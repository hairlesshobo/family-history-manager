using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Utilities;
using FoxHollow.FHM.Shared.Utilities.Serialization;
using ImageMagick;
using ImageMagick.Formats;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;

namespace FoxHollow.FHM.Core.Operations;

public class ProcessPhotosOperation
{
    private IServiceProvider _services;
    private ILogger _logger;
    private AppConfig _config;

    public bool Simulation { get; set; } = false;
    public bool RescanKnownCamDirs { get; set; } = true;

    public ProcessPhotosOperation(IServiceProvider provider)
    {
        _services = provider;

        _logger = provider.GetRequiredService<ILogger<ProcessPhotosOperation>>();
        _config = provider.GetRequiredService<AppConfig>();
    }

    public async Task StartAsync(CancellationToken ctk)
    {
        var processor = new PhotoProcessor(_services)
        {
            Directory = _config.Photos.Tiff.Directories.Root,
            Recursive = false,
            ThumbnailSize = _config.Photos.ThumbnailSize,
            ThumbnailExtension = _config.Photos.ThumbnailExtension,
            PreviewSize = _config.Photos.PreviewSize,
            PreviewExtension = _config.Photos.PreviewExtension,
            IncludePaths = _config.Photos.Tiff.Directories.Include.ToList(),
            ExcludePaths = _config.Photos.Tiff.Directories.Exclude.ToList(),
            IncludeExtensions = _config.Photos.Tiff.Directories.Extensions.ToList()
        };

        await processor.ProcessPhotos(ctk);
    }

}
