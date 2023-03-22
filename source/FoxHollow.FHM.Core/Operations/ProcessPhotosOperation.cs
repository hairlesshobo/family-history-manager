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

    public async Task<ActionQueue> StartAsync(CancellationToken cToken)
    {
        _logger.LogInformation("Starting to process photos");

        try
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

            var queue = await processor.ProcessPhotos(cToken);

            _logger.LogInformation("Photo processing complete!");

            return queue;
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Photo processing CANCELLED!");
            return new ActionQueue();
        }
    }

}
