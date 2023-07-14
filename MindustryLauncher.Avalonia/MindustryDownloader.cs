using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MindustryLauncher;

public static class MindustryDownloader
{
    private const string MindustryReleasesUrl = "https://github.com/Anuken/Mindustry/releases";

    /// <summary>
    /// Gets a list of releases
    /// </summary>
    /// <param name="count">The count of releases to get (most recent first)</param>
    /// <returns>An array with the available versions</returns>
    public static Version[] GetVersions(int count = -1)
    {
        int pages = count == -1 ? 1 : (int) MathF.Ceiling(count / 10);

        HttpClient client = new();
        List<Version> versions = new();

        Version? oldestVersion = null;
        for (int page = 0; page < pages; page++)
        {
            Version[] versionsOfPage = GetVersionPage(client, oldestVersion);

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

    private static Version[] GetVersionPage(HttpClient client, Version? after = null)
    {
        string url =
            after == null
                ? "https://github.com/Anuken/Mindustry/tags"
                : $"https://github.com/Anuken/Mindustry/tags?after=v{after}";

        Task<HttpResponseMessage> task = client.GetAsync(url);
        task.Wait();

        string result = task.Result.Content.ReadAsStringAsync().Result;

        MatchCollection matches = Regex.Matches(result, "<a href=\"\\/Anuken\\/Mindustry\\/releases\\/tag\\/v(\\d+(?:\\.\\d+)?)\" data-view-component=\"true\" class=\"Link--primary\">v\\1<\\/a>");

        Version[] versions = new Version[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            versions[i] = Version.Parse(matches[i].Groups[1].Value);
        }

        return versions;
    }

    public static async Task<bool> DownloadVersion(string path, Version version, bool downloadServer = false)
    {
        try
        {
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(MindustryReleasesUrl +
                                                                 (downloadServer
                                                                     ? $"/download/v{version}/server-release.jar"
                                                                     : $"/download/v{version}/Mindustry.jar")
            );

            FileStream fileStream = File.Open(path, FileMode.Create);

            await response.Content.CopyToAsync(fileStream);

            await fileStream.FlushAsync();

            fileStream.Close();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}