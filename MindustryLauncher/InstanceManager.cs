using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MindustryLauncher;

public static class InstanceManager
{
    public static List<Instance> Instances { get; private set; }


    private static string GetDataPath() => $"{LauncherPath()}/mindustrylauncher.json";

    private static string LauncherPath() => "C:/Users/zeno/Desktop/MindustryLauncher";

    public static void LoadInstances()
    {
        if (!File.Exists(GetDataPath()))
        {
            Instances = new();
            return;
        }

        string dataText = File.ReadAllText(GetDataPath());
        InstanceData? instanceData = JsonSerializer.Deserialize<InstanceData>(dataText);

        if (instanceData == null)
        {
            Console.WriteLine("Unable to load instances: invalid file");
            return;
        }

        Instances = instanceData.Instances.ToList();
    }

    public static void Save()
    {
        Directory.CreateDirectory(LauncherPath());

        string dataString = JsonSerializer.Serialize(new InstanceData()
        {
            Instances = Instances.ToArray(),
        });

        File.WriteAllText(GetDataPath(), dataString);
    }

    public static void CreateInstance(string instanceName, Version version)
    {
        string instancePath = Path.Join(LauncherPath(), "instances", instanceName);
        Directory.CreateDirectory(instancePath);
        MindustryDownloader.DownloadVersion(Path.Join(instancePath, "mindustry.jar"), version).ContinueWith(downloadTask =>
        {
            if(downloadTask.Result)
                ExtractIcon(instancePath);
            // Hand over the result of the download task to the next task
            return downloadTask.Result;
        }).ContinueWith(OnInstanceDownloaded);


        MainWindow.MainWindowInstance.SetStatus($"Downloading mindustry v{version}...");

        Instance instance = new()
        {
            Name = instanceName,
            Version = version,
            Path = instancePath,
        };

        Instances.Add(instance);
    }

    private static void ExtractIcon(string instancePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(Path.Join(instancePath, "mindustry.jar"));

        ZipArchiveEntry entry = archive.GetEntry("icons/icon.ico")!;

        entry.ExtractToFile(Path.Join(instancePath, "icon.ico"));
    }

    private static void OnInstanceDownloaded(Task<bool> task)
    {
        bool success = task.Result;
        MainWindow.MainWindowInstance.SetStatus("");
        MainWindow.MainWindowInstance.UpdateInstanceList();
    }

    public static void DeleteInstance(Instance i)
    {
        Directory.Delete(i.Path, true);
        Instances.Remove(i);
        MainWindow.MainWindowInstance.UpdateInstanceList();
    }

    public static void RunInstance(Instance i)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = JavaPath,
            Arguments = "-jar " + Path.Join(i.Path, "mindustry.jar"),
            RedirectStandardOutput = true,
        };
        startInfo.EnvironmentVariables["appdata"] = i.Path;

        Process? process = Process.Start(startInfo);

        if (process is not {HasExited: false})
            return;

        i.IsRunning = true;
        i.Process = process;

        process.Exited += (sender, args) => { i.IsRunning = false; };
    }

    private const string JavaPath = "javaw";
}

public class InstanceData
{
    public Instance[] Instances { get; set; }
}