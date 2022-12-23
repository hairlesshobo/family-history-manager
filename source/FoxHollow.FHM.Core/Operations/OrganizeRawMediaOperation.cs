using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Interop;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Core.Operations
{
    public class OrganizeRawMediaOperation
    {
        private IServiceProvider _services;
        private ILogger _logger;
        
        public bool Simulation { get; set; } = false;
        public bool RescanKnownCamDirs { get; set; } = true;

        public OrganizeRawMediaOperation(IServiceProvider provider)
        {
            _services = provider;
            _logger = provider.GetRequiredService<ILogger<OrganizeRawMediaOperation>>();
        }

        public async Task StartAsync(CancellationToken ctk)
        {
            var rawVideoUtils = _services.GetRequiredService<RawVideoUtils>();
            var treeWalkerFactory = _services.GetRequiredService<TreeWalkerFactory>();

            List<string> knownCamNames = AppInfo.CameraProfiles.Select(x => x.Name).ToList();

            var treeWalker = treeWalkerFactory.GetWalker(AppInfo.Config.Directories.Raw.Root);
            treeWalker.IncludePaths = new List<string>(AppInfo.Config.Directories.Raw.Include);
            treeWalker.ExcludePaths = new List<string>(AppInfo.Config.Directories.Raw.Exclude);
            treeWalker.IncludeExtensions = new List<string>(AppInfo.Config.Directories.Raw.Extensions);

            var stats = new Dictionary<string, uint>
            {
                {"LocatedAtInvalidLevel", 0},
                {"InProperDirectory", 0},
                {"InProperDirectoryTrusted", 0},
                {"InCamDirNotCorrectCam", 0},
                {"NotInDirMovable", 0},
                {"NotInDirNotMovable", 0},
                {"InUnknownCamDirMovable", 0},
                {"InUnknownCamDirNotMovable", 0}
            };

            await foreach (var collection in treeWalker.StartScanAsync())
            {
                var entries = collection.Entries.Where(x => x.Ignored == false);

                if (entries.Count() > 1)
                    throw new Exception("More than one non-ignore media file entries, unknown how to proceed!");

                var entry = entries.First();

                //! TODO: status update here
                _logger.LogInformation($"{entry.RelativeDepth}: {entry.Path}");


                // now we build some logic to to determine if the video is organized properly
                // into a known camera folder. camera folders currently live at a depth of 3
                bool inCamDir = entry.RelativeDepth == 3;
                bool dirIsKnownCam = false;
                string camDirName = "";

                // if we are deemed to be in a cam folder, lets check to see if it is a known camera
                if (inCamDir)
                    camDirName = entry.FileInfo.Directory.Name;

                dirIsKnownCam = knownCamNames.Contains(camDirName);

                if (dirIsKnownCam && !this.RescanKnownCamDirs)
                {
                    _logger.LogDebug("File is in a known cam dir, and not instructed to rescan, nothing to do.");
                    //! TODO: status update here
                    continue;
                }

                // Generate sidecar files
                //! TODO: status update here
                var sidecar = await rawVideoUtils.LoadOrGenerateAsync(entry.Path, true);

                // Identify camera
                //! TODO: status update here
                var pyop = new PyIdentifyCamera(_services, entry.Path);
                var identifyCamResult = await pyop.RunAsync(ctk);
                pyop.Dispose();


                // Analyze 
                if (entry.RelativeDepth != 2 && entry.RelativeDepth != 3)
                {
                    //! TODO: Status update here
                    stats["LocatedAtInvalidLevel"] += 1;
                    _logger.LogInformation($"Media file exists at unknown depth: {entry.RelativeDepth}");
                    continue;
                }


                // The file was located in a camera directory, which may or may not be known
                if (inCamDir)
                {
                    // the cam directory is a known camera profile
                    if (dirIsKnownCam)
                    {
                        // The scanner confidently identified the camera of the file
                        if (identifyCamResult.ConfidencePass)
                        {
                            // The identified camera matches the name of the dir, all done here!
                            if (identifyCamResult.IdentifiedCamName == camDirName)
                            {
                                stats["InProperDirectory"] += 1;
                                _logger.LogInformation("File in proper directory, nothing to do");
                            }

                            // TODO: Do we want to auto rename if the dir is named wrong but we confidently know the cam profile?
                            // The identified camera does NOT match the name of the dir, that means we have a mis-named directory
                            // that needs to be corrected manually by the user
                            else
                            {
                                stats["InCamDirNotCorrectCam"] += 1;
                                _logger.LogInformation("File in known cam dir, but cam not same as identified");
                            }
                        }

                        // The file is in a known camera dir but our scanner is not confident enough to confirm it. This means
                        // the file was organized manually by the user and the resuls must be trusted
                        else
                        {
                            stats["InProperDirectoryTrusted"] += 1;
                            _logger.LogInformation("File in known cam directory but no confidence match, assume correct and nothing to do.");
                        }
                    }

                    // the cam directory is NOT a known camera profile
                    else
                    {
                        // The camera scanner confidence is high enough to rename the directory automatically
                        if (identifyCamResult.ConfidencePass)
                        {
                            // rename the directory to the new name
                            if (!this.Simulation)
                            {
                                stats["InProperDirectory"] += 1;
                                collection.RenameDir(identifyCamResult.IdentifiedCamName);
                                _logger.LogInformation($"File in unknown cam directory.. dir renamed to \'{identifyCamResult.IdentifiedCamName}\'");
                            }

                            else
                            {
                                stats["InUnknownCamDirMovable"] += 1;
                                _logger.LogInformation($"File in unknown cam directory.. dir to be renamed to \'{identifyCamResult.IdentifiedCamName}\'");
                            }
                        }

                        // the camera scanner confidence is NOT high enough to take any automatic action. manual user intervention required
                        else
                        {
                            stats["InUnknownCamDirNotMovable"] += 1;
                            _logger.LogInformation("File in unknown cam directory.. dir cannot be renamed automatically");
                        }
                    }
                }

                // Discovered file not located in an appropriately named camera directory.. action needs to be taken
                else
                {
                    // We are confident of our camera identification, move the file to an appropriately named folder
                    if (identifyCamResult.ConfidencePass)
                    {
                        if (this.Simulation)
                        {
                            stats["NotInDirMovable"] += 1;
                            _logger.LogInformation($"File not in cam directory, can automatically move to cam dir '{identifyCamResult.IdentifiedCamName}'");
                        }
                        else
                        {
                            string newDirectory = Path.Join(entry.FileInfo.DirectoryName, identifyCamResult.IdentifiedCamName);

                            if (!Path.Exists(newDirectory))
                                Directory.CreateDirectory(newDirectory);

                            else if (!Directory.Exists(newDirectory))
                            {
                                stats["NotInDirNotMovable"] += 1;
                                _logger.LogWarning($"File not in cam directory, but path '{identifyCamResult.IdentifiedCamName}' exists and is not directory!");
                                return;
                            }

                            collection.MoveCollection(newDirectory);

                            stats["InProperDirectory"] += 1;
                            _logger.LogInformation($"File not in cam directory, moved to cam dir '{identifyCamResult.IdentifiedCamName}'");
                        }
                    }

                    // Our camera confidence is not high enough to automatically move the file, user must move manually
                    else
                    {
                        stats["NotInDirNotMovable"] += 1;
                        _logger.LogInformation("File not in cam directory, and must be moved to cam dir manually");
                    }
                }

                // break;
            }

            _logger.LogInformation($"Summary:");
            _logger.LogInformation($"----------------------------------------------------");
            _logger.LogInformation($"      located_at_invalid_level: {stats["LocatedAtInvalidLevel"]}");
            _logger.LogInformation($"           in_proper_directory: {stats["InProperDirectory"]}");
            _logger.LogInformation($"   in_proper_directory_trusted: {stats["InProperDirectoryTrusted"]}");
            _logger.LogInformation($"    in_cam_dir_not_correct_cam: {stats["InCamDirNotCorrectCam"]}");
            _logger.LogInformation($"    in_unknown_cam_dir_movable: {stats["InUnknownCamDirMovable"]}");
            _logger.LogInformation($"in_unknown_cam_dir_not_movable: {stats["InUnknownCamDirNotMovable"]}");
            _logger.LogInformation($"            not_in_dir_movable: {stats["NotInDirMovable"]}");
            _logger.LogInformation($"        not_in_dir_not_movable: {stats["NotInDirNotMovable"]}");
        }
    }
}