using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using MindustryLauncher.Avalonia;
using MindustryLauncher.Avalonia.Instances;

namespace MindustryLauncher;

public static class InstanceManager
{
    
    public static string GetDataPath() => $"{Program.LauncherPath()}/mindustrylauncher.json";
    
    public static void CreateInstance(string instanceName, Version version)
    {
        string instancePath = Path.Join(Program.LauncherPath(), "instances", instanceName);
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

        DataManager.Data.Instances.Add(instance);
        DataManager.Save();
    }

    public static void CreateLocalServerInstance(string instanceName, Version version)
    {
        string instancePath = Path.Join(Program.LauncherPath(), "instances", instanceName);
        Directory.CreateDirectory(instancePath);
        MindustryDownloader.DownloadVersion(Path.Join(instancePath, "mindustry-server.jar"), version, true).ContinueWith(OnInstanceDownloaded);


        MainWindow.MainWindowInstance.SetStatus($"Downloading mindustry server v{version}...");

        LocalServerInstance instance = new()
        {
            Name = instanceName,
            Version = version,
            Path = instancePath,
        };

        DataManager.Data.Instances.Add(instance);
        DataManager.Save();
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