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
using FoxHollow.FHM.Classes;
using FoxHollow.FHM.Shared.Services;
using Microsoft.Extensions.Logging;
using Splat;

namespace FoxHollow.FHM.Views;

public partial class MainWindowView : Window
{
    private Microsoft.Extensions.Logging.ILogger _logger;
    private IEventLoggerEventService _eventLoggerService;
    private TextBox _logBox;

    public MainWindowView()
    {
        InitializeComponent();

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

        var dialog = new AboutWindowView();
        await dialog.ShowDialog(this);
    }

    private void ShowTiffProcessor(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation("Showing tiff processor window");

        var dialog = new ProcessTiffWindowView();
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