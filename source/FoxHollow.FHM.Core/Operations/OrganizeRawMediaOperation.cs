using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Interop;
using FoxHollow.FHM.Shared.Interop.Models;
using FoxHollow.FHM.Shared.Models.Video;
using FoxHollow.FHM.Shared.Utilities.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FoxHollow.FHM.Core.Operations
{
    public class OrganizeRawMediaOperation
    {
        public bool Simulation { get; set; } = true;
        public bool RescanKnownCamDirs { get; set; } = true;

        public async Task StartAsync(CancellationToken ctk)
        {
            List<string> knownCamNames = AppInfo.CameraProfiles.Select(x => x.Name).ToList();

            var scanner = new TreeWalker(AppInfo.Config.Directories.Raw.Root);
            scanner.IncludePaths = new List<string>(AppInfo.Config.Directories.Raw.Include);
            scanner.ExcludePaths = new List<string>(AppInfo.Config.Directories.Raw.Exclude);
            scanner.Extensions = new List<string>(AppInfo.Config.Directories.Raw.Extensions);

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

            await foreach (var entry in scanner.StartScanAsync())
            {
                //! TODO: status update here
                Console.WriteLine($"{entry.RelativeDepth}: {entry.Path}");

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
                    //! TODO: status update here
                    continue;
                }

                // Generate sidecar files
                //! TODO: status update here
                var sidecar = await RawSidecar.LoadOrGenerateAsync(entry.Path, true);

                // Identify camera
                //! TODO: status update here
                var pyop = new PyIdentifyCamera(entry.Path);
                var identifyCamResult = await pyop.RunAsync(ctk);
                pyop.Dispose();


                // Analyze 
                if (entry.RelativeDepth != 2 && entry.RelativeDepth != 3)
                {
                    //! TODO: Status update here
                    stats["LocatedAtInvalidLevel"] += 1;
                    Console.WriteLine($"Media file exists at unknown depth: {entry.RelativeDepth}");
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
                                Console.WriteLine("File in proper directory, nothing to do");
                            }

                            // TODO: Do we want to auto rename if the dir is named wrong but we confidently know the cam profile?
                            // The identified camera does NOT match the name of the dir, that means we have a mis-named directory
                            // that needs to be corrected manually by the user
                            else
                            {
                                stats["InCamDirNotCorrectCam"] += 1;
                                Console.WriteLine("File in known cam dir, but cam not same as identified");
                            }
                        }

                        // The file is in a known camera dir but our scanner is not confident enough to confirm it. This means
                        // the file was organized manually by the user and the resuls must be trusted
                        else
                        {
                            stats["InProperDirectoryTrusted"] += 1;
                            Console.WriteLine("File in known cam directory but no confidence match, assume correct and nothing to do.");
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
                                // rename_dir(identifyCamResult.IdentifiedCamName)
                                Console.WriteLine($"File in unknown cam directory.. dir renamed to \'{identifyCamResult.IdentifiedCamName}\'");
                            }

                            else
                            {
                                stats["InUnknownCamDirMovable"] += 1;
                                Console.WriteLine($"File in unknown cam directory.. dir to be renamed to \'{identifyCamResult.IdentifiedCamName}\'");
                            }
                        }

                        // the camera scanner confidence is NOT high enough to take any automatic action. manual user intervention required
                        else
                        {
                            stats["InUnknownCamDirNotMovable"] += 1;
                            Console.WriteLine("File in unknown cam directory.. dir cannot be renamed automatically");
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
                            Console.WriteLine("File not in cam directory, can automatically move to cam dir \'{identified_cam_name}\'");
                        }
                        else
                        {
                            string new_directory = Path.Join(entry.FileInfo.DirectoryName, identifyCamResult.IdentifiedCamName);

                            if (!Path.Exists(new_directory))
                                Directory.CreateDirectory(new_directory);
                            else if (!Directory.Exists(new_directory))
                            {
                                stats["NotInDirNotMovable"] += 1;
                                Console.WriteLine($"File not in cam directory, but path '{identifyCamResult.IdentifiedCamName}' exists and is not directory!");
                                return;
                            }

                            string new_path = Path.Join(new_directory, entry.FileInfo.Name);

                            // TODO: Build logic to also move any sidecar files that may be present
                            // media_file.path_info.rename(new_path)

                            stats["InProperDirectory"] += 1;
                            Console.WriteLine($"File not in cam directory, moved to cam dir '{identifyCamResult.IdentifiedCamName}'");
                        }
                    }

                    // Our camera confidence is not high enough to automatically move the file, user must move manually
                    else
                    {
                        stats["NotInDirNotMovable"] += 1;
                        Console.WriteLine("File not in cam directory, and must be moved to cam dir manually");
                    }
                }

                // break;
            }

            Console.WriteLine($"Summary:");
            Console.WriteLine($"----------------------------------------------------");
            Console.WriteLine($"      located_at_invalid_level: {stats["LocatedAtInvalidLevel"]}");
            Console.WriteLine($"           in_proper_directory: {stats["InProperDirectory"]}");
            Console.WriteLine($"   in_proper_directory_trusted: {stats["InProperDirectoryTrusted"]}");
            Console.WriteLine($"    in_cam_dir_not_correct_cam: {stats["InCamDirNotCorrectCam"]}");
            Console.WriteLine($"    in_unknown_cam_dir_movable: {stats["InUnknownCamDirMovable"]}");
            Console.WriteLine($"in_unknown_cam_dir_not_movable: {stats["InUnknownCamDirNotMovable"]}");
            Console.WriteLine($"            not_in_dir_movable: {stats["NotInDirMovable"]}");
            Console.WriteLine($"        not_in_dir_not_movable: {stats["NotInDirNotMovable"]}");
        }
    }
}