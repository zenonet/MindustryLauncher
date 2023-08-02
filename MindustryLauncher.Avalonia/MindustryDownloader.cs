using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MindustryLauncher.Avalonia;

namespace MindustryLauncher;

public static class MindustryDownloader
{
    private const string MindustryReleasesUrl = "https://github.com/Anuken/Mindustry/releases";

    private static readonly HttpClientHandler HttpClientHandler = new();
    private static readonly ProgressMessageHandler ProgressHandler = new(HttpClientHandler);
    private static readonly HttpClient HttpClient = new(HttpClientHandler);

    /// <summary>
    /// Gets a list of releases
    /// </summary>
    /// <param name="count">The count of releases to get (most recent first)</param>
    /// <returns>An array with the available versions</returns>
    public static Version[] GetVersions(int count = -1)
    {
        int pages = count == -1 ? 1 : (int) MathF.Ceiling(count / 10);

        List<Version> versions = new();

        Version? oldestVersion = null;
        for (int page = 0; page < pages; page++)
        {
            Version[] versionsOfPage = GetVersionPage(oldestVersion);

            oldestVersion = versionsOfPage.Last();

            versions.AddRange(versionsOfPage);
        }

        if (count != -1)
        {
            // Ensure the list contains exactly count versions
            versions.RemoveRange(count, versions.Count - count);
        }

        return versions.ToArray();
    }

    public static Version[] GetVersionPage(Version? after = null)
    {
        string url =
            after == null
                ? "https://github.com/Anuken/Mindustry/tags"
                : $"https://github.com/Anuken/Mindustry/tags?after=v{after}";

        Task<HttpResponseMessage> task = HttpClient.GetAsync(url);
        task.Wait();

        string result = task.Result.Content.ReadAsStringAsync().Result;

        MatchCollection matches = Regex.Matches(result,
            "<a href=\"\\/Anuken\\/Mindustry\\/releases\\/tag\\/v(\\d+(?:\\.\\d+)?)\" .*?>v\\1<\\/a>");

        Version[] versions = new Version[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            versions[i] = Version.Parse(matches[i].Groups[1].Value);
        }

        return versions;
    }

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

    public static int GetVersionCount()
    {
        Task<HttpResponseMessage> task = HttpClient.GetAsync("https://github.com/Anuken/Mindustry/");
        task.Wait();

        string result = task.Result.Content.ReadAsStringAsync().Result;

        Match match = Regex.Match(result,
            "Releases\\s.*?title=\"(\\d+)");

        return int.Parse(match.Groups[1].Value);
    }
}