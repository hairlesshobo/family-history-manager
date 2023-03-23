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
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FoxHollow.FHM.Classes;
using FoxHollow.FHM.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;

namespace FoxHollow.FHM.Views;

public partial class MainWindowView : WindowBase
{
    private Microsoft.Extensions.Logging.ILogger _logger;
    private IEventLoggerEventService _eventLoggerService;
    private TextBox _logBox;

    public MainWindowView(IServiceProvider services)
        : base(services)
    {

        InitializeComponent();

        // TODO: eliminate splat?
        // AvaloniaLocator.Current.GetRequiredService

        if (_eventLoggerService == null)
            _eventLoggerService = Locator.Current.GetRequiredService<IEventLoggerEventService>();

        if (_logger == null)
            _logger = Locator.Current.GetRequiredService<ILogger<MainWindowView>>();
    }

    private void LogCallback(string level, string message)
    {
        // _logBox.Text = message + _logBox.Text;
        _logBox.Text += message;
        _logBox.CaretIndex = _logBox.Text.Length;
    }

    private async void ShowAboutWindow(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Showing about window");

        var dialog = ActivatorUtilities.CreateInstance<AboutWindowView>(_services);
        await dialog.ShowDialog(this);
    }

    private void ShowTiffProcessor(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Showing tiff processor window");

        var dialog = ActivatorUtilities.CreateInstance<ProcessTiffWindowView>(_services);
        dialog.Show(this);
    }

    protected override void OnOpened(EventArgs e)
    {
        _logBox = this.GetControl<TextBox>("logBox");
        _eventLoggerService.RegisterLogDestination(LogCallback);
        _logger.LogInformation("Family History Manager initialized");

        base.OnOpened(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _eventLoggerService.UnregisterLogDestination(LogCallback);
        _logBox = null;

        base.OnClosing(e);
    }
}