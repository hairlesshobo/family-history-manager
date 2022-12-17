using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Classes;

namespace FoxHollow.FHM.Core.Operations
{
    public class GenerateRawMediaSidecarOperation
    {
        public async Task StartAsync(CancellationToken cToken)
        {
            var scanner = new FileScanner(AppInfo.Config.Directories.Raw.Root);
            scanner.IncludePaths = new List<string>(AppInfo.Config.Directories.Raw.Include);
            scanner.ExcludePaths = new List<string>(AppInfo.Config.Directories.Raw.Exclude);
            scanner.Extensions = new List<string>(AppInfo.Config.Directories.Raw.Extensions);

            await foreach (var entry in scanner.StartScanAsync())
            {
                Console.WriteLine($"{entry.RelativeDepth}: {entry.Path}");
            }
        }
    }
}