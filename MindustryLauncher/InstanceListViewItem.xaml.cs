using System.Windows.Controls;

namespace MindustryLauncher;

public partial class InstanceListViewItem : ListViewItem
{
    public Instance Instance;
    public InstanceListViewItem(Instance instance)
    {
        InitializeComponent();

        Instance = instance;
        Name.Text = instance.Name;
    }
}