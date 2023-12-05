using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using MindustryLauncher.Avalonia;
using MindustryLauncher.Avalonia.Models;
using MindustryLauncher.Avalonia.Windows;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace MindustryLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow MainWindowInstance = null!;

        public MainWindowViewModel Data => (DataContext as MainWindowViewModel)!;

        public MainWindow()
        {
            MainWindowInstance = this;
            InitializeComponent();
            Data.UpdateLatestVersionInfo();

            InstanceList.ItemsSource = DataManager.Data.Instances;

            DeleteInstanceButton.Click += DeleteInstance;
            AddInstanceButton.Click += AddInstance;
            AddServerInstanceButton.Click += AddServerInstance;
            OpenMindustryFolderButton.Click += OpenMindustryFolder;
            OpenServerWindowButton.Click += OpenServerWindow;
        }

        public ServerManagementWindow? ServerManagementWindow;

        private void OpenServerWindow(object? sender, RoutedEventArgs e)
        {
            if (ServerManagementWindow?.Server == Data.SelectedInstance) return;

            ServerManagementWindow?.Close();

            ServerManagementWindow = new((ServerInstance) Data.SelectedInstance!);
            ServerManagementWindow.Show();
        }

        private void OpenMindustryFolder(object? sender, RoutedEventArgs e)
        {
            if (Data.SelectedInstance is not ILocalInstance instance)
                return;

            string mindustryPath = Path.Join(instance.Path, "Mindustry");
            if (!Directory.Exists(mindustryPath))
                Directory.CreateDirectory(mindustryPath);

            using Process folderOpener = new();
            folderOpener.StartInfo.FileName = mindustryPath;
            folderOpener.StartInfo.UseShellExecute = true;
            folderOpener.Start();
        }

        private void DeleteInstance(object? sender, RoutedEventArgs e)
        {
            // Buffer the selected instance in case another is selected
            Instance i = Data.SelectedInstance!;
            // Create a msg box
            IMsBox<ButtonResult>? confirmationBox = MessageBoxManager.GetMessageBoxStandard(new()
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                ContentTitle = "Are you sure?",
                ContentMessage = $"This will delete the instance {Data.SelectedInstance!.Name} and all its files.\nThis action is irreversible",
                Topmost = true,
            });
            confirmationBox.ShowWindowAsync().ContinueWith(result =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (result.Result != ButtonResult.Ok)
                        return;

                    i.DeleteInstance();
                });
            });
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            DataManager.Save();
        }

        private void AddInstance(object? sender, RoutedEventArgs e)
        {
            NewInstanceWindow window = new();
            window.Show();
        }

        private void AddServerInstance(object? sender, RoutedEventArgs e)
        {
            NewServerInstanceWindow window = new();
            window.Show();
        }

        private void SetRunButtonText(object? sender = null, int _2 = 0)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (sender == null || sender == Data.SelectedInstance)
                    RunButton.Content = "Run";
            });
        }

        void SetStopButtonText(object? sender = null, object? _2 = null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (sender == null || sender == Data.SelectedInstance)
                    RunButton.Content = "Stop";
            });
        }
    }
}