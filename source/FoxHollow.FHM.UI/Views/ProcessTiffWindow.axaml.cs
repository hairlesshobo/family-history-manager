using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splat;

namespace FoxHollow.FHM.UI.Views;

public partial class ProcessTiffWindow : Window
{
    private Microsoft.Extensions.Logging.ILogger _logger;

    public ProcessTiffWindow()
    {
        InitializeComponent();

        _logger = Locator.Current.GetService<ILogger<ProcessTiffWindow>>();

        var eventLoggerService = Program.Services.GetRequiredService<IEventLoggerEventService>();

        var logBox = this.GetControl<TextBox>("logBox");

        Action<string, string> logCallback = (level, message) =>
        {
            logBox.Text += message;
        };

        eventLoggerService.RegisterLogDestination(logCallback);
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
    }
}
