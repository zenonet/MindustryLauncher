using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using MindustryLauncher.Avalonia;

namespace MindustryLauncher;

public class LocalClientInstance : Instance, IFolderOpenable
{

    public string Path { get; set; }
    public static LocalClientInstance Parse(string json)
    {
        return JsonSerializer.Deserialize<LocalClientInstance>(json) ?? throw new InvalidOperationException("The provided json instance was invalid");
    }

    [NonSerialized] public Process? Process;

    [field: NonSerialized] public override bool IsRunning { get; protected set; }

    public override void Run()
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

        process.BeginOutputReadLine();

        process.EnableRaisingEvents = true;

        IsRunning = true;
        Process = process;

        process.Exited += (_, _) =>
        {
            IsRunning = false;
            OnInstanceExited(process.ExitCode);
        };

        OnInstanceStarted();
    }

    public override void Kill()
    {
        Process?.Kill();
    }

    public override void DeleteInstance()
    {
        Directory.Delete(Path, true);
        DataManager.Data.Instances.Remove(this);
        MainWindow.MainWindowInstance.UpdateInstanceList();
    }

    private const string JavaPath = "java";
}