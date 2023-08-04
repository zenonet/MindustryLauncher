using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindustryLauncher.Avalonia.Models;

public partial class PlayerModel : ObservableObject
{
    public Action<string> SendCommand;
    
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string uuid;

    [RelayCommand]
    private void Kick()
    {
        SendCommand($"kick {Name}");
    }

    [RelayCommand]
    private void Ban()
    {
        SendCommand($"ban {Name}");
    }

    [RelayCommand]
    private void MakeAdmin()
    {
        throw new NotImplementedException();
        //SendCommand($"admin  {Uuid}");
    }
}