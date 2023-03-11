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
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.UI.Classes;
using FoxHollow.FHM.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;

namespace FoxHollow.FHM.UI.Views;

public partial class ProcessTiffWindow : Window
{
    private Microsoft.Extensions.Logging.ILogger _logger;
    private IEventLoggerEventService _eventLoggerService;
    private TextBox _logBox;

    public ProcessTiffWindow()
    {
        InitializeComponent();
    }

    private void LogCallback(string level, string message)
    {
        _logBox.Text += message;
    }

    protected override void OnInitialized()
    {
        if (_logger == null)
            _logger = Locator.Current.GetRequiredService<ILogger<ProcessTiffWindow>>();

        if (_eventLoggerService == null)
            _eventLoggerService = Locator.Current.GetRequiredService<IEventLoggerEventService>();

        base.OnInitialized();
    }

    protected override void OnOpened(EventArgs e)
    {
        _logBox = this.GetControl<TextBox>("logBox");

        _eventLoggerService.RegisterLogDestination(LogCallback);

        // TODO: This is for debugging only
        ((ProcessTiffWindowViewModel)this.DataContext).ParentWindow = this;

        base.OnOpened(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _eventLoggerService.UnregisterLogDestination(LogCallback);
        _logBox = null;

        base.OnClosing(e);
    }

    private async void OnClick_SelectDirectory(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation("meow");

        var dialog = new OpenFolderDialog();
        var result = await dialog.ShowAsync(this);

        if (result != null)
        {
            var context = (ProcessTiffWindowViewModel)this.DataContext;
            context.RootDirectory = result;
        }

        // TODO: Once Avalonia 11 is released, this will be used instead
        // var result = await this.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        // {
        //     AllowMultiple = false
        // });

        // if (result?.Count > 0)
        // {
        //     var context = (ProcessTiffWindowViewModel)this.DataContext;
        //     context.RootDirectory = result[0].Path.AbsolutePath;
        // }
    }
}
