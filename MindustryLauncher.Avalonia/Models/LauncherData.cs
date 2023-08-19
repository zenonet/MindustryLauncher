using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MindustryLauncher.Avalonia.Models;

public class LauncherData
{
    public static LauncherData CreateDefault()
    {
        return new()
        {
            Instances = new(),
            JavaPath = "java"
        };
        
    }

    public ObservableCollection<Instance> Instances { get; set; }
    public string JavaPath { get; set; }
}