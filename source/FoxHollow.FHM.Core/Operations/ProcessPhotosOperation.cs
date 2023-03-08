using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared.Classes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Core.Operations;

public class ProcessPhotosOperation
{
    private IServiceProvider _services;
    private ILogger _logger;
    private AppConfig _config;

    public bool Recursive { get; set; } = false;
    public string RootDirectory { get; set; }

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
            Directory = this.RootDirectory,
            Recursive = this.Recursive,
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
