using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;

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
        HttpClient client = new();

        Task<HttpResponseMessage> task = client.GetAsync("https://github.com/Anuken/Mindustry/tags");
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

    public static async Task<bool> DownloadVersion(string path, Version version)
    {
        try
        {
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(MindustryReleasesUrl + $"/download/v{version}/Mindustry.jar");
            
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