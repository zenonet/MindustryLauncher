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

        public ObservableCollection<InstanceListBoxItem> ListBoxItems { get; } = new();

        public MainWindowViewModel Data => (DataContext as MainWindowViewModel)!;
        public MainWindow()
        {
            MainWindowInstance = this;
            InitializeComponent();
            Data.UpdateLatestVersionInfo();
            
            UpdateInstanceList();
            
            InstanceList.SelectionChanged += OnSelectionChanged;
            DeleteInstanceButton.Click += DeleteInstance;
            AddInstanceButton.Click += AddInstance;
            AddServerInstanceButton.Click += AddServerInstance;
            OpenMindustryFolderButton.Click += OpenMindustryFolder;
            OpenServerWindowButton.Click += OpenServerWindow;
        }

        private ServerManagementWindow? serverManagementWindow;
        private void OpenServerWindow(object? sender, RoutedEventArgs e)
        {
            if (serverManagementWindow == null)
            {
                goto openServerManagementWindow;
            }
            if (serverManagementWindow.Server == Data.SelectedInstance)
            {
                return;
            }

            if (serverManagementWindow.Server != Data.SelectedInstance)
            {
                serverManagementWindow.Close();
            }

            openServerManagementWindow:
            serverManagementWindow = new((ServerInstance) Data.SelectedInstance!);
            serverManagementWindow.Show();
        }

        private void OpenMindustryFolder(object? sender, RoutedEventArgs e)
        {
            if (Data.SelectedInstance is not IFolderOpenable instance)
                return;
            
            string mindustryPath = Path.Join(instance.Path, "Mindustry");
            if(!Directory.Exists(mindustryPath))
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
                ContentMessage = $"This will delete the instance {Data.SelectedInstance!.Name} and all its files.\nThis action is not undoable",
                Topmost = true,
            });
            confirmationBox.ShowWindowAsync().ContinueWith(result =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (result.Result != ButtonResult.Ok)
                        return;

                    if (Data.SelectedInstance == i && InstanceIconLarge.Source is Bitmap b)
                    {
                        // Allow deletion of the instance by closing the icon
                        b.Dispose();
                    }

                    i.DeleteInstance();
                    UpdateInstanceList();
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

        public void UpdateInstanceList()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ListBoxItems.Clear();

                if (Data.SelectedInstance != null && !DataManager.Data.Instances.Contains(Data.SelectedInstance))
                    Data.SelectedInstance = null;

                foreach (Instance i in DataManager.Data.Instances)
                {
                    InstanceListBoxItem listViewItem = new(i);
                    ListBoxItems.Add(listViewItem);

                    if (i == Data.SelectedInstance)
                        InstanceList.SelectedItem = listViewItem;
                }

                InstanceList.ItemsSource = ListBoxItems;
            });
        }
        
        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SetRunButtonText();

            if (Data.SelectedInstance != null)
            {
                Data.SelectedInstance.InstanceStarted -= SetStopButtonText;
                Data.SelectedInstance.InstanceExited -= SetRunButtonText;
            }

            
            Data.SelectedInstance = ((InstanceListBoxItem) InstanceList.SelectedItem!).Instance;
            //InstanceName.Text = Data.SelectedInstance.Name;
            //InstanceVersion.Text = Data.SelectedInstance.Version.ToString();

            if (Data.SelectedInstance is LocalClientInstance instance)
            {
                try
                {
                    InstanceIconLarge.Source = new Bitmap(Path.Join(instance.Path, "icon.ico"));
                }
                catch
                {
                    InstanceIconLarge.Source = null;
                }
            }
            else
            {
                // Clear the icon if there is no icon for this instance
                if (InstanceIconLarge.Source is Bitmap b)
                {
                    b.Dispose();
                }
            }

            // Update the text on the run button according to the instances status
            Data.SelectedInstance.InstanceStarted += SetStopButtonText;
            Data.SelectedInstance.InstanceExited += SetRunButtonText;

            if (Data.SelectedInstance.IsRunning)
                SetStopButtonText();

            OpenServerWindowButton.IsVisible = Data.SelectedInstance is ServerInstance;
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

        public void SetStatus(string s)
        {
            Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = s; });
        }
    }
}