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

    [NonSerialized] public Process? Process;

    [NonSerialized] public bool IsRunning;

    public void Run()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = JavaPath,
            Arguments = "-jar " + System.IO.Path.Join(Path, "mindustry.jar"),
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        startInfo.EnvironmentVariables["appdata"] = Path;
        startInfo.EnvironmentVariables["XDG_DATA_HOME"] = Path;

        Process? process = Process.Start(startInfo);

        if (process is null || process.HasExited)
            return;

        process.EnableRaisingEvents = true;

        IsRunning = true;
        Process = process;

        process.Exited += (_, _) =>
        {
            IsRunning = false;
            OnInstanceExited.Invoke(this, process.ExitCode);
        };

        OnInstanceStarted.Invoke(this, EventArgs.Empty);
    }

    private const string JavaPath = "java";

    #region Events

    public event EventHandler OnInstanceStarted = delegate { };
    public event EventHandler<int> OnInstanceExited = delegate { };

    #endregion
}