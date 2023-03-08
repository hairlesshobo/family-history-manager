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