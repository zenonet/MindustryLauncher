using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using MindustryLauncher.Avalonia;
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
        private Instance? selectedInstance;

        public Instance? SelectedInstance
        {
            get
            {
                if (InstanceManager.Instances.Contains(selectedInstance))
                    return selectedInstance;

                if (InstanceManager.Instances.Count <= 0)
                    return null;

                selectedInstance = InstanceManager.Instances[0];
                return selectedInstance;
            }
            set
            {
                if (value == null)
                {
                    //int nextInstanceIndex = (InstanceManager.Instances.IndexOf(selectedInstance) + 1) % InstanceManager.Instances.Count;

                    selectedInstance = InstanceManager.Instances.Count > 0
                        ? InstanceManager.Instances[0]
                        : null;
                }
                else
                    selectedInstance = value;


                InstanceOptionsStackPanel.IsEnabled = selectedInstance != null;
            }
        }

        public static MainWindow MainWindowInstance = null!;

        public ObservableCollection<InstanceListBoxItem> ListBoxItems { get; } = new();

        public MainWindow()
        {
            MainWindowInstance = this;
            InitializeComponent();

            InstanceManager.LoadInstances();
            UpdateInstanceList();


            InstanceList.SelectionChanged += OnSelectionChanged;
            RunButton.Click += HandleRunButtonClick;
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
            if (serverManagementWindow.Server == selectedInstance)
            {
                return;
            }

            if (serverManagementWindow.Server != SelectedInstance)
            {
                serverManagementWindow.Close();
            }

            openServerManagementWindow:
            serverManagementWindow = new((ServerInstance) SelectedInstance!);
            serverManagementWindow.Show();
        }

        private void OpenMindustryFolder(object? sender, RoutedEventArgs e)
        {
            if (SelectedInstance is not IFolderOpenable instance)
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
            Instance i = SelectedInstance!;
            // Create a msg box
            IMsBox<ButtonResult>? confirmationBox = MessageBoxManager.GetMessageBoxStandard(new()
            {
                ButtonDefinitions = ButtonEnum.OkCancel,
                ContentTitle = "Are you sure?",
                ContentMessage = $"This will delete the instance {SelectedInstance!.Name} and all its files.\nThis action is not undoable",
                Topmost = true,
            });
            confirmationBox.ShowWindowAsync().ContinueWith(result =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (result.Result != ButtonResult.Ok)
                        return;

                    if (selectedInstance == i && InstanceIconLarge.Source is Bitmap b)
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
            InstanceManager.Save();
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

                if (SelectedInstance != null && !InstanceManager.Instances.Contains(SelectedInstance))
                    SelectedInstance = null;

                foreach (Instance i in InstanceManager.Instances)
                {
                    InstanceListBoxItem listViewItem = new(i);
                    ListBoxItems.Add(listViewItem);

                    if (i == SelectedInstance)
                        InstanceList.SelectedItem = listViewItem;
                }

                InstanceList.ItemsSource = ListBoxItems;
            });
        }

        private void HandleRunButtonClick(object? sender, RoutedEventArgs e)
        {
            if (SelectedInstance == null)
                return;

            if (SelectedInstance.IsRunning)
                SelectedInstance.Kill();
            else
                SelectedInstance?.Run();
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SetRunButtonText();

            if (SelectedInstance != null)
            {
                SelectedInstance.InstanceStarted -= SetStopButtonText;
                SelectedInstance.InstanceExited -= SetRunButtonText;
            }

            SelectedInstance = ((InstanceListBoxItem) InstanceList.SelectedItem!).Instance;
            InstanceName.Text = SelectedInstance.Name;
            InstanceVersion.Text = SelectedInstance.Version.ToString();

            if (SelectedInstance is LocalClientInstance instance)
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
            SelectedInstance.InstanceStarted += SetStopButtonText;
            SelectedInstance.InstanceExited += SetRunButtonText;

            if (SelectedInstance.IsRunning)
                SetStopButtonText();

            OpenServerWindowButton.IsVisible = SelectedInstance is ServerInstance;
        }

        private void SetRunButtonText(object? sender = null, int _2 = 0)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (sender == null || sender == SelectedInstance)
                    RunButton.Content = "Run";
            });
        }

        void SetStopButtonText(object? sender = null, object? _2 = null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (sender == null || sender == SelectedInstance)
                    RunButton.Content = "Stop";
            });
        }

        public void SetStatus(string s)
        {
            Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = s; });
        }
    }
}