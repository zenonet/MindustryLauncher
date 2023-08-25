using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindustryLauncher.Avalonia.Models;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyInstanceSelected))]
    [NotifyPropertyChangedFor(nameof(RunStopButtonText))]
    [NotifyPropertyChangedFor(nameof(IsSelectedInstanceServer))]
    [NotifyPropertyChangedFor(nameof(InstanceIcon))]
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
    
    public bool IsSelectedInstanceServer => SelectedInstance is ServerInstance;
    public Bitmap? InstanceIcon => SelectedInstance is LocalClientInstance ? new Bitmap(Path.Join(((ILocalInstance)SelectedInstance).Path, "icon.ico")) : null;

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
            VersionCache.CacheVersions().Wait();
            VersionCache.LoadVersions();

            OnPropertyChanged(nameof(LatestVersion));
        });
    }

    
    partial void OnSelectedInstanceChanging(Instance? value)
    {
        void OnInstanceExited(object? sender, int i)
        {
            OnPropertyChanged(nameof(RunStopButtonText));
        }

        void OnInstanceStarted(object? sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(RunStopButtonText));
        }

        if (selectedInstance != null)
        {
            selectedInstance.InstanceStarted -= OnInstanceStarted;
            selectedInstance.InstanceExited -= OnInstanceExited;
        }

        if (value != null)
        {
            value.InstanceStarted += OnInstanceStarted;
            value.InstanceExited += OnInstanceExited;
        }
        
        
    }
}