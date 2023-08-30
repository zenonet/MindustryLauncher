using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using MindustryLauncher.Avalonia.Models;
using MindustryLauncher.Avalonia.Windows;
using Newtonsoft.Json;

namespace MindustryLauncher.Avalonia;

public static class DataManager
{
    public static LauncherData Data => (Application.Current!.DataContext as LauncherData)!;

    public static string GetDataPath() => $"{Program.LauncherPath()}/mindustrylauncher.json";

    public static void Save()
    {
        string dataString = JsonConvert.SerializeObject(Application.Current!.DataContext,
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
            Application.Current!.DataContext = LauncherData.CreateDefault();
            return;
        }

        string text = File.ReadAllText(GetDataPath());
        Application.Current!.DataContext = JsonConvert.DeserializeObject<LauncherData>(text, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        }) ?? LauncherData.CreateDefault();

        IEnumerable<Instance> compromisedInstances = from instance in Data.Instances
            where instance is ILocalInstance
            where !((ILocalInstance) instance).VerifyIntegrity()
            select instance;

        List<Instance> compromisedInstancesList = compromisedInstances.ToList();
        if (compromisedInstancesList.Any())
        {
            InstanceCompromisedWindow compromisedWindow = new(new(compromisedInstancesList.ToList()));
            compromisedWindow.Show();
        }
    }
}