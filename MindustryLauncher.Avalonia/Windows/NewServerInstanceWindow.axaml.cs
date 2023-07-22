using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MindustryLauncher.Avalonia.Instances;

namespace MindustryLauncher.Avalonia;

public partial class NewServerInstanceWindow : Window
{
    public NewServerInstanceWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        
        versionsInDropdown = new();
        LoadAvailableVersions();

        
        Local_VersionDropDown.ItemsSource = versionsInDropdown;
        RemoteDocker_VersionDropDown.ItemsSource = versionsInDropdown;
        
        Local_CreateButton.Click += OnLocalCreateButtonClicked;
        RemoteDocker_CreateButton.Click += OnRemoteDockerCreateButtonClicked;

        RemoteDocker_TabItem.IsVisible = false;
    }



    private void OnLocalCreateButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (Local_InstanceName.Text!.Contains(' '))
        {
            Local_InstanceNameErrorMessage.Content = "The instance name must not contain spaces";
            return;
        }
        
        InstanceManager.CreateLocalServerInstance(Local_InstanceName.Text!, Version.Parse(((ComboBoxItem) Local_VersionDropDown.SelectedItem!).Content!.ToString()!));
        this.Close();
    }
    
    private void OnRemoteDockerCreateButtonClicked(object? sender, RoutedEventArgs e)
    {
        RemoteDockerServerInstance i = new();

        i.Ip = RemoteDocker_SshIp.Text!;
        i.Usernsame = RemoteDocker_SshUserName.Text!;
        i.Password = RemoteDocker_SshPassword.Text!;
        i.ContainerName = RemoteDocker_ContainerName.Text!;
        i.Version = Version.Parse(((ComboBoxItem)RemoteDocker_VersionDropDown.SelectedItem!).Content!.ToString()!);
        
        InstanceManager.Instances.Add(i);
        MainWindow.MainWindowInstance.UpdateInstanceList();
        this.Close();
    }
    
    private ObservableCollection<ComboBoxItem> versionsInDropdown;

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