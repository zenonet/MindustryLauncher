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
    
    [ObservableProperty]
    private string statusText = "";
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowProgress))]
    private float progressValue;
    
    public bool ShowProgress => ProgressValue is > 0 and < 100;

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


    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task UpdateLatestVersionInfo()
    {
        await Task.Run(() =>
        {
            VersionCache.CheckForNewVersions();
            VersionCache.LoadVersions();

            OnPropertyChanged(nameof(LatestVersion));
        });
    }
}