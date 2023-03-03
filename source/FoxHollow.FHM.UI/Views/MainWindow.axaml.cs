using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FoxHollow.FHM.Core.Operations;
using Splat;

namespace FoxHollow.FHM.UI.Views;

public partial class MainWindow : Window
{
    public static TextBox LogBox { get; private set; }
    public MainWindow()
    {
        InitializeComponent();

        MainWindow.LogBox = this.GetControl<TextBox>("logBox");
    }
}