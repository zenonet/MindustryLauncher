using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace MindustryLauncher.Avalonia;

public static class VersionCache
{
    public static string CachePath => Path.Join(Program.LauncherPath(), "versions.cache");

    public static Version[]? Versions { get; private set; }

    public static async Task CacheVersions()
    {
        StringBuilder sb = new();

        GitHubClient client = new(new ProductHeaderValue("MindustryLauncher"));
        IReadOnlyList<RepositoryTag>? tags = await client.Repository.GetAllTags("Anuken", "Mindustry");

        if(Version.Parse(tags[0].Name) == Versions?[0])
            return;
        
        foreach (RepositoryTag tag in tags)
        {
            Version v = Version.Parse(tag.Name);
            sb.Append(v);
            sb.Append(';');
        }

        Console.WriteLine($"Saving cache to {Path.GetFullPath(CachePath)}");
        StreamWriter sw = File.CreateText(CachePath);
        await sw.WriteAsync(sb);
        sw.Close();
    }

    public static Version[] GetAllCachedVersions()
    {
        if (Versions == null)
            LoadVersions();

        return Versions![..];
    }

    public static void LoadVersions()
    {
        if (!File.Exists(CachePath))
            CacheVersions().Wait();

        string rawCache = File.ReadAllText(CachePath);
        string[] versionStrings = rawCache.Split(';', StringSplitOptions.RemoveEmptyEntries);
        Version[] versions = new Version[versionStrings.Length];

        for (int i = 0; i < versionStrings.Length; i++)
        {
            versions[i] = Version.Parse(versionStrings[i]);
        }

        Versions = versions;
    }
}