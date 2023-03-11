/**
 *  Family History Manager - https://code.foxhollow.cc/fhm/
 *
 *  A cross platform tool to help organize and preserve all types
 *  of family history
 * 
 *  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
 *
 *  This Source Code Form is subject to the terms of the Mozilla Public
 *  License, v. 2.0. If a copy of the MPL was not distributed with this
 *  file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using FoxHollow.FHM.Core.Operations;
using FoxHollow.FHM.Shared.Native;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting.Internal;
using ReactiveUI;
using Splat;

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
    public Window ParentWindow { get; set; }


    /// <summary>
    ///     Default constructor
    /// </summary>
    public ProcessTiffWindowViewModel()
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

        ProcessPhotosOperation operation = new ProcessPhotosOperation(Program.Services)
        {
            Recursive = this.RecursiveScan,
            RootDirectory = this.RootDirectory
        };

        // _config.Photos.Tiff.Directories.Root

        this.IsRunning = true;
        // await operation.StartAsync(_cts.Token);

        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams()
            {
                ContentTitle = "Message!!",
                ContentMessage = "This is the body. ooo yeah!",
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

            await messageBoxStandardWindow.ShowDialog(this.ParentWindow);

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
