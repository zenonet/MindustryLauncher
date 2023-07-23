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
        
        Local_CreateButton.Click += OnLocalCreateButtonClicked;
        RemoteDocker_CreateButton.Click += OnRemoteDockerCreateButtonClicked;

        RemoteDocker_TabItem.IsVisible = false;
        //RemoteDocker_VersionDropDown.LoadVersions(10);
        
        Local_VersionDropDown.LoadVersions(10);
    }



    private void OnLocalCreateButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (Local_InstanceName.Text!.Contains(' '))
        {
            Local_InstanceNameErrorMessage.Content = "The instance name must not contain spaces";
            return;
        }
        
        InstanceManager.CreateLocalServerInstance(Local_InstanceName.Text!, Local_VersionDropDown.SelectedVersion!.Value);
        this.Close();
    }
    
    private void OnRemoteDockerCreateButtonClicked(object? sender, RoutedEventArgs e)
    {
        RemoteDockerServerInstance i = new();

        i.Ip = RemoteDocker_SshIp.Text!;
        i.Usernsame = RemoteDocker_SshUserName.Text!;
        i.Password = RemoteDocker_SshPassword.Text!;
        i.ContainerName = RemoteDocker_ContainerName.Text!;
        i.Version = RemoteDocker_VersionDropDown.SelectedVersion!.Value;
        
        InstanceManager.Instances.Add(i);
        MainWindow.MainWindowInstance.UpdateInstanceList();
        this.Close();
    }
}