using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
        this.
        InstanceNameTextBlock.Text = instance.Name;
    }
    /*
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }*/
}