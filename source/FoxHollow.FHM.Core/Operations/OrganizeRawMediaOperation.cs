//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Interop;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Core.Operations;

public class OrganizeRawMediaOperation
{
    //* Folder Depths:
    //* 0: root
    //* 1: year [2022]
    //* 2: event [2022-12-25 -- Christmas]
    //* 3: scene [Sony HVR-Z1U]

    private IServiceProvider _services;
    private ILogger _logger;
    private AppConfig _config;

    public bool Simulation { get; set; } = false;
    public bool RescanKnownCamDirs { get; set; } = true;

    public OrganizeRawMediaOperation(IServiceProvider provider)
    {
        _services = provider;
        _logger = provider.GetRequiredService<ILogger<OrganizeRawMediaOperation>>();
        _config = provider.GetRequiredService<AppConfig>();
    }

    public async Task StartAsync(CancellationToken ctk)
    {
        //* overview:
        //*   - sort raw media files and create sidecars
        //*   - remove empty directories
        //*   - create scene metadata
        //*   - create event metadata

        ActionQueue queue = null;

        // // Organize the raw video folders, grouping by capture camera
        // queue = await this.OrganizeVideosByCamera(ctk);

        // // TODO: find a way to yield to caller for verification before continuing to next step
        // if (!this.Simulation)
        // {
        //     _logger.LogInformation("Beginning to apply raw camera organization actions!");
        //     await queue.ExecuteAll(ctk);
        // }


        // if (queue.Executed)
        // {
        // Create metadata for all valid scenes
        queue = await this.CreateSceneMetadata(ctk);

        // TODO: find a way to yield to caller for verification before continuing to next step
        if (!this.Simulation)
        {
            _logger.LogInformation("Beginning to apply scene metadata actions!");
            await queue.ExecuteAll(ctk);
        }
    }
    // }

    private async Task<ActionQueue> OrganizeVideosByCamera(CancellationToken ctk)
    {
        // TODO: actually use the cancellation token
        var actionQueue = new ActionQueue();

        var rawVideoUtils = _services.GetRequiredService<RawVideoUtils>();
        var treeWalkerFactory = _services.GetRequiredService<MediaTreeWalkerFactory>();
        var camProfileService = _services.GetRequiredService<CamProfileService>();

        var treeWalker = treeWalkerFactory.GetWalker(_config.Directories.Raw.Root);
        treeWalker.IncludePaths = new List<string>(_config.Directories.Raw.Include);
        treeWalker.ExcludePaths = new List<string>(_config.Directories.Raw.Exclude);
        treeWalker.IncludeExtensions = new List<string>(_config.Directories.Raw.Extensions);

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
                camDirName = entry.FileEntry.Directory.Name;

            dirIsKnownCam = camProfileService.CamNames.Contains(camDirName);

            if (dirIsKnownCam && !this.RescanKnownCamDirs)
            {
                _logger.LogDebug("File is in a known cam dir, and not instructed to rescan, nothing to do.");
                //! TODO: status update here
                continue;
            }

            // Generate sidecar files
            //! TODO: status update here
            var sidecar = await rawVideoUtils.LoadOrGenerateRawVideoMetadataAsync(entry.Path, true);

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

            string eventDirPath = null;

            if (entry.RelativeDepth > 2)
            {
                DirectoryInfo eventDirInfo = PathUtils.FindDirectoryAtRelativeDepth(collection.RootDirectoryPath, entry.FileEntry.Directory.FullName, 2);
                eventDirPath = eventDirInfo.FullName;
            }

            Action moveCollectionToCamDir = () =>
            {
                string newDirectory = Path.Join(eventDirPath, identifyCamResult.IdentifiedCamName);

                actionQueue.Add(
                    collection,
                    $"Move collection '{collection}' from '{collection.Directory.FullName}' to '{newDirectory}'",
                    (action, ctk) =>
                    {
                        collection.MoveCollection(newDirectory, true);
                        _logger.LogInformation($"Moved '{collection}' from '{collection.Directory.FullName}' to '{newDirectory}'");
                    }
                );
            };


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

                        // The collection is in a known cam dir, but not camera that was confidently
                        // identified.. move to the correct location
                        else
                            moveCollectionToCamDir();
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
                        moveCollectionToCamDir();

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
                    moveCollectionToCamDir();

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

        return actionQueue;
    }

    private async Task<ActionQueue> CreateSceneMetadata(CancellationToken ctk)
    {
        var actionQueue = new ActionQueue();

        var treeWalkerFactory = _services.GetRequiredService<MediaTreeWalkerFactory>();
        var mediainfoUtils = _services.GetRequiredService<MediainfoUtils>();
        // var camProfileService = _services.GetRequiredService<CamProfileService>();

        // \u002B30.3571-081.8092/
        // +30.3571-081.8092/

        var treeWalker = treeWalkerFactory.GetWalker(_config.Directories.Raw.Root);
        treeWalker.IncludePaths = new List<string>(_config.Directories.Raw.Include);
        treeWalker.ExcludePaths = new List<string>(_config.Directories.Raw.Exclude);
        treeWalker.IncludeExtensions = new List<string>(_config.Directories.Raw.Extensions);

        using (_logger.BeginScope("CreateSceneMetadata"))
        {
            foreach (var scene in treeWalker.FindRawVideoScenes())
            {
                //! TODO: status update here
                _logger.LogDebug(scene.Path);

                if (scene.CameraProfile == null)
                {
                    _logger.LogTrace($"Skipped scene '{scene.RootRelativePath}' because '{scene.CameraName}' is not a known camera");
                    continue;
                }

                var sceneMetadata = new RawVideoScene()
                {
                    Camera = scene.CameraProfile.Name,
                    CameraID = scene.CameraProfile.ID,
                    Location = new RawVideoSceneLocation()
                };

                // look for GPS coordinates
            }
        }

        return actionQueue;
    }
}