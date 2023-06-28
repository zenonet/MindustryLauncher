using System;
using System.Diagnostics;
using System.Text.Json;

namespace MindustryLauncher;

public class Instance
{
    public string Name { get; set; }

    public string Path { get; set; }

    public Version Version { get; set; }

    public static Instance Parse(string json)
    {
        return JsonSerializer.Deserialize<Instance>(json) ?? throw new InvalidOperationException("The provided json instance was invalid");
    }

    [NonSerialized]
    public Process? Process;

    [NonSerialized]
    public bool IsRunning;
}