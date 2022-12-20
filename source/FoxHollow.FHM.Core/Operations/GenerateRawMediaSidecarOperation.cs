using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Interop;
using FoxHollow.FHM.Shared.Interop.Models;
using FoxHollow.FHM.Shared.Models.Video;
using FoxHollow.FHM.Shared.Utilities.Serialization;

namespace FoxHollow.FHM.Core.Operations
{
    public class GenerateRawMediaSidecarOperation
    {
        public async Task StartAsync(CancellationToken ctk)
        {
            var scanner = new TreeWalker(AppInfo.Config.Directories.Raw.Root);
            scanner.IncludePaths = new List<string>(AppInfo.Config.Directories.Raw.Include);
            scanner.ExcludePaths = new List<string>(AppInfo.Config.Directories.Raw.Exclude);
            scanner.Extensions = new List<string>(AppInfo.Config.Directories.Raw.Extensions);

            await foreach (var entry in scanner.StartScanAsync())
            {
                Console.WriteLine($"{entry.RelativeDepth}: {entry.Path}");

                var sidecar = await RawSidecar.LoadOrGenerateAsync(entry.Path, true);

                string profilesDir = Path.Combine(SysInfo.ConfigRoot, "profiles");

                using (var pyop = new PythonInterop<IdentifyCameraProgress, IdentifyCameraResult>("identify-camera", profilesDir, entry.Path))
                {
                    var result = await pyop.RunAsync(ctk);

                    Console.WriteLine(result.IdentifiedCamName);
                }

                break;
            }
        }
    }
}