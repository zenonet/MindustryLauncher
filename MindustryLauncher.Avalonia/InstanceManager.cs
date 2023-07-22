using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using MindustryLauncher.Avalonia;
using MindustryLauncher.Avalonia.Instances;
using Newtonsoft.Json;

namespace MindustryLauncher;

public static class InstanceManager
{
    public static List<Instance> Instances { get; private set; }


    private static string GetDataPath() => $"{LauncherPath()}/mindustrylauncher.json";

    public static string LauncherPath() => "MindustryLauncher";

    public static void LoadInstances()
    {
        if (!File.Exists(GetDataPath()))
        {
            Instances = new();
            return;
        }

        string dataText = File.ReadAllText(GetDataPath());
        InstanceData? instanceData = JsonConvert.DeserializeObject<InstanceData>(dataText, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

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

        string dataString = JsonConvert.SerializeObject(
            new InstanceData
            {
                Instances = Instances.ToArray(),
            },
            Formatting.Indented,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }
        );

        File.WriteAllText(GetDataPath(), dataString);
    }

    public static void CreateInstance(string instanceName, Version version)
    {
        string instancePath = Path.Join(LauncherPath(), "instances", instanceName);
        Directory.CreateDirectory(instancePath);
        MindustryDownloader.DownloadVersion(Path.Join(instancePath, "mindustry.jar"), version).ContinueWith(downloadTask =>
        {
            if (downloadTask.Result)
                ExtractIcon(instancePath);
            // Hand over the result of the download task to the next task
            return downloadTask.Result;
        }).ContinueWith(OnInstanceDownloaded);


        MainWindow.MainWindowInstance.SetStatus($"Downloading mindustry v{version}...");

        LocalClientInstance instance = new()
        {
            Name = instanceName,
            Version = version,
            Path = instancePath,
        };

        Instances.Add(instance);
    }

    public static void CreateLocalServerInstance(string instanceName, Version version)
    {
        string instancePath = Path.Join(LauncherPath(), "instances", instanceName);
        Directory.CreateDirectory(instancePath);
        MindustryDownloader.DownloadVersion(Path.Join(instancePath, "mindustry-server.jar"), version, true).ContinueWith(OnInstanceDownloaded);


        MainWindow.MainWindowInstance.SetStatus($"Downloading mindustry server v{version}...");

        LocalServerInstance instance = new()
        {
            Name = instanceName,
            Version = version,
            Path = instancePath,
        };

        Instances.Add(instance);
    }

    private static void ExtractIcon(string instancePath)
    {
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(Path.Join(instancePath, "mindustry.jar"));

            ZipArchiveEntry? entry;
            entry = archive.GetEntry("icons/icon.ico")!;
            if (entry != null)
                goto extraction;
            entry = archive.GetEntry("sprites/icon.png");
            if (entry != null)
                goto extraction;

            return;

            extraction:
            entry.ExtractToFile(Path.Join(instancePath, entry.Name));
        }
        catch
        {
            // ignore
        }
    }

    private static void OnInstanceDownloaded(Task<bool> task)
    {
        bool success = task.Result;
        MainWindow.MainWindowInstance.SetStatus("");
        MainWindow.MainWindowInstance.UpdateInstanceList();
    }
}

public class InstanceData
{
    public Instance[] Instances { get; set; } = null!;
}