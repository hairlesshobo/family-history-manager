using System;
using Avalonia.Controls;
using FoxHollow.FHM.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var eventLoggerService = Program.Services.GetRequiredService<IEventLoggerEventService>();

        var logBox = this.GetControl<TextBox>("logBox");

        Action<string, string> logCallback = (level, message) =>
        {
            logBox.Text += message;
        };

        eventLoggerService.RegisterLogDestination(logCallback);
    }
}