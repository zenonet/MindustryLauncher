using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace MindustryLauncher.Avalonia;

public partial class InstanceListBoxItem : UserControl
{
    public Instance? Instance
    {
        get => GetValue(InstanceProperty);
        set => SetValue(InstanceProperty, value);
    }

    public static readonly StyledProperty<Instance?> InstanceProperty =
        AvaloniaProperty.Register<InstanceListBoxItem, Instance?>(nameof(Instance));

    public InstanceListBoxItem()
    {
        InitializeComponent();
        
        InstanceProperty.Changed.Subscribe(args =>
        {
            if (args.OldValue.Value != null)
            {
                args.OldValue.Value.InstanceStarted -= OnInstanceStarted;
                args.OldValue.Value.InstanceExited -= OnInstanceExited;
            }

            Init();
        });
    }

    public InstanceListBoxItem(Instance instance)
    {
        InitializeComponent();

        Instance = instance;

        Init();
    }

    private void Init()
    {
        if(Instance == null)
            return;
        InstanceNameTextBlock.Text = Instance.Name + (Instance is ServerInstance ? " (Server)" : "");

        Instance.InstanceStarted += OnInstanceStarted;
        Instance.InstanceExited += OnInstanceExited;
    }

    private void OnInstanceStarted(object? _, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => { StatusTextBlock.Text = "running"; });
    }

    private void OnInstanceExited(object? _, int exitCode)
    {
        Dispatcher.UIThread.InvokeAsync(() => { StatusTextBlock.Text = $"exited with exit code {exitCode}"; });
    }
}