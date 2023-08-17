using System;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading.Tasks;
using MindustryLauncher.Avalonia;

namespace MindustryLauncher;

public static class MindustryDownloader
{
    private const string MindustryReleasesUrl = "https://github.com/Anuken/Mindustry/releases";

    private static readonly HttpClientHandler HttpClientHandler = new();
    private static readonly ProgressMessageHandler ProgressHandler = new(HttpClientHandler);
    private static readonly HttpClient HttpClient = new(HttpClientHandler);

    private static string GetDownloadUrl(Version version, bool isServer = false)
    {
        string downloadUrl = MindustryReleasesUrl + $"/download/v{version}/";

        // Pick the right file name based on the version
        if (isServer)
            return downloadUrl + "server-release.jar";

        if (version > Version.MinVersionWithNewBuildName)
            return downloadUrl + "Mindustry.jar";

        return downloadUrl + "desktop-release.jar";
    }

    private static void OnDownloadProgressChanged(float progress)
    {
        MainWindow.MainWindowInstance.Data.ProgressValue = progress;
    }

    public static async Task<bool> DownloadVersion(string path, Version version, bool downloadServer = false)
    {
        try
        {
            string downloadUrl = GetDownloadUrl(version, downloadServer);
            await HttpClient.DownloadFile(downloadUrl, path, new Progress<float>(OnDownloadProgressChanged));
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}