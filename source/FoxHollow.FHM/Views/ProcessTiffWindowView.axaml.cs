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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FoxHollow.FHM.Classes;
using FoxHollow.FHM.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;

namespace FoxHollow.FHM.Views;

public partial class ProcessTiffWindowView : Window
{
    private Microsoft.Extensions.Logging.ILogger _logger;

    public ProcessTiffWindowView()
    {
        InitializeComponent();

        if (_logger == null)
            _logger = Locator.Current.GetRequiredService<ILogger<ProcessTiffWindowView>>();

        this.DataContext = new ProcessTiffWindowViewModel();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
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
