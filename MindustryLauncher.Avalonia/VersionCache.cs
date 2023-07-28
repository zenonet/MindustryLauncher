using System;
using System.IO;
using System.Text;

namespace MindustryLauncher.Avalonia;

public static class VersionCache
{
    public static string CachePath => Path.Join(Program.LauncherPath(), "versions.cache");

    public static Version[]? Versions { get; private set; }


    public static void CheckForNewVersions()
    {
        Version[] latestVersions = MindustryDownloader.GetVersionPage();

        Version latestCachedVersion = GetAllCachedVersions()[0];

        Version latestVersion = latestVersions[0];

        if (latestCachedVersion != latestVersion)
        {
            CacheVersions(latestCachedVersion);
        }
    }

    public static void CacheVersions(Version? until = null, Action<Version[]>? onProgressChanged = null)
    {
        Version? after = null;

        StringBuilder sb = new();

        while (true)
        {
            Version[] versionsOnPage = MindustryDownloader.GetVersionPage(after);

            if (versionsOnPage.Length == 0)
                break;

            after = versionsOnPage[^1];

            onProgressChanged?.Invoke(versionsOnPage);

            foreach (Version version in versionsOnPage)
            {
                if (version == until)
                {
                    break;
                }

                sb.Append(version.ToString());
                sb.Append(';');
            }

            // If there are less than 10 versions on the page,
            // we can assume that we have reached the end of the list
            if (versionsOnPage.Length < 10)
                break;
        }

        StreamWriter sw = File.CreateText(CachePath);
        sw.Write(sb);
        sw.Close();
    }

    public static Version[] GetAllCachedVersions()
    {
        if (Versions == null)
            LoadVersions();

        return Versions![..];
    }

    private static void LoadVersions()
    {
        if (!File.Exists(CachePath))
            CacheVersions();

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