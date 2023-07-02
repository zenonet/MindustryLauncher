using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MindustryLauncher.Avalonia;

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

        public List<InstanceListBoxItem> ListBoxItems { get; } = new();

        public MainWindow()
        {
            MainWindowInstance = this;
            InitializeComponent();

            InstanceManager.LoadInstances();
            UpdateInstanceList();


            InstanceList.SelectionChanged += OnSelectionChanged;
            RunButton.Click += RunCurrentInstance;
            DeleteInstanceButton.Click += DeleteInstance;
            AddInstanceButton.Click += AddInstance;
            OpenMindustryFolderButton.Click += OpenMindustryFolder;
        }

        private void OpenMindustryFolder(object? sender, RoutedEventArgs e)
        {
            string mindustryPath = Path.Join(SelectedInstance!.Path, "Mindustry");

            using Process folderOpener = new();
            folderOpener.StartInfo.FileName = Path.GetDirectoryName(mindustryPath);
            folderOpener.StartInfo.UseShellExecute = true;
            folderOpener.Start();
        }

        private void DeleteInstance(object? sender, RoutedEventArgs e)
        {
            /*
            MessageBoxResult result = MessageBox.Show(this, "Deleting the instance will delete all its data.\nThis action is not undoable!", "Are you sure?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {*/
            InstanceManager.DeleteInstance(SelectedInstance!);
            //}
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            InstanceManager.Save();
        }

        private void AddInstance(object? sender, RoutedEventArgs e)
        {
            NewInstanceWindow window = new();
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

                InstanceList.Items = ListBoxItems;
            });
        }

        private void RunCurrentInstance(object? sender, RoutedEventArgs e)
        {
            SelectedInstance?.Run();
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SelectedInstance = ((InstanceListBoxItem) InstanceList.SelectedItem!).Instance;
            InstanceName.Text = SelectedInstance.Name;
            InstanceVersion.Text = SelectedInstance.Version.ToString();
            // TODO: Fix the icon loading preventing the deletion of the instance because the icon is still loaded
            InstanceIconLarge.Source = new Bitmap(Path.Join(SelectedInstance.Path, "icon.ico"));
        }

        public void SetStatus(string s)
        {
            Dispatcher.UIThread.InvokeAsync(() => { StatusText.Text = s; });
        }
    }
}