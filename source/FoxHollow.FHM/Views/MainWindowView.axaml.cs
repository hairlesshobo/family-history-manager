using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace FoxHollow.FHM.Views;

public partial class MainWindowView : Window
{
    public MainWindowView()
    {
        InitializeComponent();
    }

    private async void ShowAboutWindow(object sender, RoutedEventArgs e)
    {
        var dialog = new AboutWindowView();
        await dialog.ShowDialog(this);
    }
}