using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Interactivity;
using FoxHollow.FHM.Core.Operations;
using FoxHollow.FHM.UI.Views;
using ReactiveUI;

namespace FoxHollow.FHM.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        TestCommand = ReactiveCommand.CreateFromTask(Test_OnClick);
    }

    public ICommand TestCommand { get; }

    public string Greeting => "Welcome to Avalonia!";

    private async Task Test_OnClick()
    {
        // var logBox = this.GetControl<TextBox>("logBox");
        // logBox.Text += "bloop";

        TestInjectionOperation operation = new TestInjectionOperation(Program.Services);

        CancellationTokenSource cts = new CancellationTokenSource();
        await operation.StartAsync(cts.Token);
    }
}
