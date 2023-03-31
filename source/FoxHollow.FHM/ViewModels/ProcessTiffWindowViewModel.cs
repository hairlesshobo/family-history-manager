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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Core.Operations;
using FoxHollow.FHM.Shared.Classes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace FoxHollow.FHM.ViewModels;

public class ProcessTiffWindowViewModel : ViewModelBase
{
    // private ILogger _logger;
    private AppConfig _config;
    private CancellationTokenSource _cts;


    public bool FormValid 
    { 
        get => formValid;
        private set => this.RaiseAndSetIfChanged(ref formValid, value); 
    }
    private bool formValid;

    public bool RecursiveScan
    {
        get => recursiveScan;
        set
        {
            this.RaiseAndSetIfChanged(ref recursiveScan, value);
            CheckFormState();
        }
    }
    private bool recursiveScan = false;

    public string RootDirectory
    {
        get => rootDirectory;
        set
        {
            this.RaiseAndSetIfChanged(ref rootDirectory, value);
            CheckFormState();
        }
    }
    private string rootDirectory = String.Empty;

    public bool IsRunning 
    { 
        get => isRunning; 
        private set => this.RaiseAndSetIfChanged(ref isRunning, value);
    }
    private bool isRunning;

    // public ICommand RunProcess { get; }
    public ICommand CancelProcess { get; }


    /// <summary>
    ///     Default constructor
    /// </summary>
    public ProcessTiffWindowViewModel(IServiceProvider services)
        : base(services)
    {
        _config = _services.GetRequiredService<AppConfig>();
        // _logger = _services.GetRequiredService<ILogger<ProcessTiffWindowViewModel>>();

        CancelProcess = ReactiveCommand.Create(CancelProcess_OnClick);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns>Task</returns>
    public async Task RunProcess(object input)
    {
        _cts = new CancellationTokenSource();

        var sr = _storage.GetRepository("local");

        var processor = new PhotoProcessor(_services)
        {
            // TODO: Add "interactive" checkbox
            // TODO: Add "keep backups" checkbox
            Recursive = this.RecursiveScan,
            Directory = sr.GetDirectory(this.RootDirectory),

            ThumbnailSize = _config.Photos.ThumbnailSize,
            ThumbnailExtension = _config.Photos.ThumbnailExtension,
            PreviewSize = _config.Photos.PreviewSize,
            PreviewExtension = _config.Photos.PreviewExtension,
            IncludePaths = _config.Photos.Tiff.Directories.Include.ToList(),
            ExcludePaths = _config.Photos.Tiff.Directories.Exclude.ToList(),
            IncludeExtensions = _config.Photos.Tiff.Directories.Extensions.ToList()
        };

        this.IsRunning = true;
        var queue = await processor.ProcessPhotos(_cts.Token);
        this.IsRunning = false;
    }

    private void CancelProcess_OnClick()
        => _cts.Cancel();

    private void CheckFormState()
    {
        this.FormValid = new Func<bool>(() =>
        {
            if (String.IsNullOrWhiteSpace(this.RootDirectory))
                return false;

            if (!Directory.Exists(this.RootDirectory))
                return false;

            return true;
        })();
    }
}
