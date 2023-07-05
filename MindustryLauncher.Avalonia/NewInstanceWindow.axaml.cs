using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MindustryLauncher.Avalonia;

public partial class NewInstanceWindow : Window
{
    private ObservableCollection<ComboBoxItem> versionsInDropdown;

    public NewInstanceWindow()
    {
        InitializeComponent();

        versionsInDropdown = new();
        VersionDropDown.ItemsSource = versionsInDropdown;

        LoadAvailableVersions();

        CreateButton.Click += CreateInstance;
    }

    private void CreateInstance(object? sender, RoutedEventArgs e)
    {
        if (InstanceName.Text.Contains(' '))
        {
            InstanceNameErrorMessage.Content = "The instance name must not contain spaces";
            return;
        }

        InstanceManager.CreateInstance(InstanceName.Text, Version.Parse(((ComboBoxItem) VersionDropDown.SelectedItem).Content.ToString()!));
        this.Close();
    }

    private void LoadAvailableVersions()
    {
        Version[] versions = MindustryDownloader.GetVersions(10);

        versionsInDropdown.Clear();

        foreach (Version v in versions)
        {
            versionsInDropdown.Add(new()
            {
                Content = v.ToString(),
            });
        }
    }
}