using System.IO;
using Avalonia;
using MindustryLauncher.Avalonia.Models;
using Newtonsoft.Json;

namespace MindustryLauncher.Avalonia;

public static class DataManager
{
    public static LauncherData Data => Application.Current.DataContext as LauncherData;

    public static string GetDataPath() => $"{Program.LauncherPath()}/mindustrylauncher.json";

    public static void Save()
    {
        string dataString = JsonConvert.SerializeObject(App.Current.DataContext,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.None,
            }
        );

        File.WriteAllText(GetDataPath(), dataString);
    }

    public static void Load()
    {
        if (!File.Exists(GetDataPath()))
        {
            App.Current.DataContext = LauncherData.CreateDefault();
            return;
        }

        string text = File.ReadAllText(GetDataPath());
        App.Current.DataContext = JsonConvert.DeserializeObject<LauncherData>(text, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        }) ?? LauncherData.CreateDefault();
    }
}