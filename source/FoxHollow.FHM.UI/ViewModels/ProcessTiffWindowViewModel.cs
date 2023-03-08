using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Interactivity;
using FoxHollow.FHM.Core.Operations;
using FoxHollow.FHM.UI.Views;
using HarfBuzzSharp;
using ReactiveUI;

namespace FoxHollow.FHM.UI.ViewModels;

public class ProcessTiffWindowViewModel : ViewModelBase
{
    private string rootDirectory = String.Empty;
    private bool recursiveScan = false;
    private bool formValid;

    public ICommand RunProcess { get; }
    public bool FormValid { get => formValid; private set => this.RaiseAndSetIfChanged(ref formValid, value); }
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

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ProcessTiffWindowViewModel()
    {
        RunProcess = ReactiveCommand.CreateFromTask(RunProcess_OnClick);
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns>Task</returns>
    private async Task RunProcess_OnClick()
    {
        ProcessPhotosOperation operation = new ProcessPhotosOperation(Program.Services);

        operation.Recursive = this.RecursiveScan;
        operation.RootDirectory = this.RootDirectory;

        // _config.Photos.Tiff.Directories.Root

        CancellationTokenSource cts = new CancellationTokenSource();
        await operation.StartAsync(cts.Token);
    }

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
