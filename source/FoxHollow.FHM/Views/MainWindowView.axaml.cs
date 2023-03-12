using System;
using System.ComponentModel;
using System.Formats.Asn1;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
        _logBox.Text = message + _logBox.Text;
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

        base.OnOpened(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _eventLoggerService.UnregisterLogDestination(LogCallback);
        _logBox = null;

        base.OnClosing(e);
    }
}