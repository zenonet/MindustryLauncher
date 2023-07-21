using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MindustryLauncher.Avalonia;

public partial class NewInstanceWindow : Window
{
    public NewInstanceWindow()
    {
        InitializeComponent();
        
        CreateButton.Click += CreateInstance;
        
        VersionComboBox.LoadVersions(10);
    }

    private void CreateInstance(object? sender, RoutedEventArgs e)
    {
        if (InstanceName.Text.Contains(' '))
        {
            InstanceNameErrorMessage.Content = "The instance name must not contain spaces";
            return;
        }

        InstanceManager.CreateInstance(InstanceName.Text, VersionComboBox.SelectedVersion!.Value);
        this.Close();
    }
}