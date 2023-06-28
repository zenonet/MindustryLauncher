using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MindustryLauncher;

public partial class NewInstanceWindow : Window
{
    public NewInstanceWindow()
    {
        InitializeComponent();

        LoadAvailableVersions();

        CreateButton.Click += CreateInstance;
    }

    private void CreateInstance(object sender, RoutedEventArgs e)
    {
        if (InstanceName.Text.Contains(' '))
        {
            InstanceNameErrorMessage.Content = "The instance name must not contain spaces";
            return;
        }
        
        InstanceManager.CreateInstance(InstanceName.Text, Version.Parse(((ComboBoxItem)VersionDropDown.SelectedItem).Content.ToString()!));
        this.Close();
    }

    private void LoadAvailableVersions()
    {
        Version[] versions = MindustryDownloader.GetVersions(10);
        
        VersionDropDown.Items.Clear();

        foreach (Version v in versions)
        {
            VersionDropDown.Items.Add(new ComboBoxItem()
            {
                Content = v.ToString(),
            });
        }
    }
}