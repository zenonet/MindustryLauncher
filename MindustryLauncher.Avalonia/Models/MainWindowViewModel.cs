using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindustryLauncher.Avalonia.Models;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyInstanceSelected))]
    [NotifyPropertyChangedFor(nameof(RunStopButtonText))]
    private Instance? selectedInstance;

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
        if(SelectedInstance!.IsRunning)
            SelectedInstance!.Kill();
        else
            SelectedInstance!.Run();
    }
}