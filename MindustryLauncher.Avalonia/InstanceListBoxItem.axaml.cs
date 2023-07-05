using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace MindustryLauncher.Avalonia;

public partial class InstanceListBoxItem : UserControl
{
    public Instance Instance;

    public InstanceListBoxItem()
    {
    }

    public InstanceListBoxItem(Instance instance)
    {
        InitializeComponent();

        Instance = instance;

        InstanceNameTextBlock.Text = instance.Name;

        Instance.OnInstanceStarted += OnInstanceStarted;
        Instance.OnInstanceExited += OnInstanceExited;
    }

    private void OnInstanceStarted(object? _, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusTextBlock.Text = "running";
        });
    }

    private void OnInstanceExited(object? _, int exitCode)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusTextBlock.Text = $"exited with exit code {exitCode}";
        });
    }
}