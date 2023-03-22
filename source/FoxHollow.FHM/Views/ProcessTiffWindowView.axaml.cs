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
