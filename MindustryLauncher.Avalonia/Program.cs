using Avalonia;
using System;
using System.IO;

namespace MindustryLauncher.Avalonia;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-createVersionCache")
        {
            Console.WriteLine("Creating cache...");
            VersionCache.CacheVersions();
            Console.WriteLine("Cache created!");
            return 0;
        }

        return BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static bool launcherPathChecked = false;

    private const string launcherPath = "MindustryLauncher";

    public static string LauncherPath()
    {
        if (!launcherPathChecked)
        {
            if (!Directory.Exists(launcherPath))
            {
                Directory.CreateDirectory(launcherPath);
            }
        }

        return launcherPath;
    }
}