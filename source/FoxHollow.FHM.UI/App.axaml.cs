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

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FoxHollow.FHM.UI.ViewModels;
using FoxHollow.FHM.UI.Views;

namespace FoxHollow.FHM.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new ProcessTiffWindow()
            {
                DataContext = new ProcessTiffWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}