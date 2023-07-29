using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MindustryLauncher.Avalonia.Setup;

namespace MindustryLauncher.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {            
            if (File.Exists(InstanceManager.GetDataPath()))
            {
                desktop.MainWindow = new MainWindow();
            }
            else
            {
                desktop.MainWindow = new SetupWindow();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}