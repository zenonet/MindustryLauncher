using System;
using System.Diagnostics;
using System.IO;

namespace MindustryLauncher.Avalonia.Instances;

public class LocalServerInstance : ServerInstance, IFolderOpenable
{
    public string Path { get; set; }

    [NonSerialized] public Process? Process;

    [field: NonSerialized] public override bool IsRunning { get; protected set; }


    public override void Run()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "java",
            Arguments = "-jar " + System.IO.Path.Join(Path, "mindustry-server.jar"),
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
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
            ServerOutput?.Dispose();
            ServerInput?.Dispose();
            ServerOutput = null;
            ServerInput = null;
            OnInstanceExited(process.ExitCode);
        };

        OnInstanceStarted();

        ServerInput = Process.StandardInput;
        ServerOutput = Process.StandardOutput;
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
}