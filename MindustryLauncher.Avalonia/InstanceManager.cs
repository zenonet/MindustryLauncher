using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Avalonia.Threading;
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
        string jarPath = Path.Join(instancePath, "mindustry.jar");

        Task.Run(async () =>
        {
            bool success = await MindustryDownloader.DownloadVersion(jarPath, version);
            if (success)
                ExtractIcon(instancePath);
            OnInstanceDownloaded(success);

            LocalClientInstance instance = new()
            {
                Name = instanceName,
                Version = version,
                Path = instancePath,
                JarHash = Utils.GetSha256OfFile(jarPath),
            };

            Dispatcher.UIThread.Invoke(() =>
            {
                DataManager.Data.Instances.Add(instance);
                DataManager.Save();
            });
        });

        Dispatcher.UIThread.InvokeAsync(() => MainWindow.MainWindowInstance.Data.StatusText = $"Downloading mindustry v{version}...");
    }

    public static void CreateLocalServerInstance(string instanceName, Version version)
    {
        string instancePath = Path.Join(Program.LauncherPath(), "instances", instanceName);
        Directory.CreateDirectory(instancePath);
        string jarPath = Path.Join(instancePath, "mindustry-server.jar");
        Task.Run(async () =>
        {
            bool success = await MindustryDownloader.DownloadVersion(jarPath, version, true);
            OnInstanceDownloaded(success);

            LocalServerInstance instance = new()
            {
                Name = instanceName,
                Version = version,
                Path = instancePath,
                JarHash = Utils.GetSha256OfFile(jarPath),
            };

            Dispatcher.UIThread.Invoke(() =>
            {
                DataManager.Data.Instances.Add(instance);
                DataManager.Save();
            });
        });


        Dispatcher.UIThread.InvokeAsync(() => MainWindow.MainWindowInstance.Data.StatusText = $"Downloading mindustry server v{version}...");
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

    private static void OnInstanceDownloaded(bool success)
    {
        Dispatcher.UIThread.InvokeAsync(() => MainWindow.MainWindowInstance.Data.StatusText = "");
    }
}

public class InstanceData
{
    public Instance[] Instances { get; set; } = null!;
}