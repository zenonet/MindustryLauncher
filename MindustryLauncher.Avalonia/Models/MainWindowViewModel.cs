using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindustryLauncher.Avalonia.Models;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyInstanceSelected))]
    [NotifyPropertyChangedFor(nameof(RunStopButtonText))]
    private Instance? selectedInstance;

    public Version LatestVersion => VersionCache.Versions[0];
    public bool IsAnyInstanceSelected => SelectedInstance != null;

    public string RunStopButtonText
    {
        get
        {
            if (SelectedInstance == null || !SelectedInstance.IsRunning)
                return "Run";
            return "Stop";
        }
    }

    [RelayCommand]
    public void StartStopSelectedInstance()
    {
        if (SelectedInstance!.IsRunning)
            SelectedInstance!.Kill();
        else
            SelectedInstance!.Run();
    }

    private Task? updateLatestVersionTask;
    [RelayCommand]
    public void UpdateLatestVersionInfo()
    {
        if(updateLatestVersionTask == null || updateLatestVersionTask.Status == TaskStatus.Running)
            return;
        updateLatestVersionTask = Task.Run(() =>
        {
            VersionCache.CheckForNewVersions();
            VersionCache.LoadVersions();
            Dispatcher.UIThread.InvokeAsync(() =>
                    OnPropertyChanged(nameof(LatestVersion)));
        });
    }
}