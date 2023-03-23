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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FoxHollow.FHM.Core.Operations;
using ReactiveUI;

namespace FoxHollow.FHM.ViewModels;

public class ProcessTiffWindowViewModel : ViewModelBase
{
    private CancellationTokenSource _cts;

    private string rootDirectory = String.Empty;
    private bool recursiveScan = false;
    private bool formValid;
    private bool isRunning;

    public bool FormValid 
    { 
        get => formValid;
        private set => this.RaiseAndSetIfChanged(ref formValid, value); 
    }
    public bool RecursiveScan
    {
        get => recursiveScan;
        set
        {
            this.RaiseAndSetIfChanged(ref recursiveScan, value);
            CheckFormState();
        }
    }
    public string RootDirectory
    {
        get => rootDirectory;
        set
        {
            this.RaiseAndSetIfChanged(ref rootDirectory, value);
            CheckFormState();
        }
    }
    public bool IsRunning 
    { 
        get => isRunning; 
        private set => this.RaiseAndSetIfChanged(ref isRunning, value);
    }

    // public ICommand RunProcess { get; }
    public ICommand CancelProcess { get; }


    /// <summary>
    ///     Default constructor
    /// </summary>
    public ProcessTiffWindowViewModel(IServiceProvider services)
        : base(services)
    {
        CancelProcess = ReactiveCommand.Create(CancelProcess_OnClick);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns>Task</returns>
    public async Task RunProcess(object input)
    {
        _cts = new CancellationTokenSource();

        ProcessPhotosOperation operation = new ProcessPhotosOperation(_services)
        {
            Recursive = this.RecursiveScan,
            RootDirectory = this.RootDirectory
            // TODO: Add "interactive" checkbox
            // TODO: Add "keep backups" checkbox
        };

        // _config.Photos.Tiff.Directories.Root

        this.IsRunning = true;
        await operation.StartAsync(_cts.Token);

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
