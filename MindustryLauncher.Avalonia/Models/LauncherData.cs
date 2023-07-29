using System.Collections.Generic;

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

    public List<Instance> Instances { get; set; }
    public string JavaPath { get; set; }
}