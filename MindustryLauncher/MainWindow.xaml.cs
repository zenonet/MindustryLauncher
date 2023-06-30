using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace MindustryLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Instance selectedInstance;

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
                    selectedInstance = InstanceManager.Instances[(InstanceManager.Instances.IndexOf(selectedInstance) + 1) % InstanceManager.Instances.Count];
                else
                    selectedInstance = value;
            }
        }

        public static MainWindow MainWindowInstance = null!;

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

        private void OpenMindustryFolder(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Path.Join(SelectedInstance!.Path, "Mindustry"));
        }

        private void DeleteInstance(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Deleting the instance will delete all its data.\nThis action is not undoable!", "Are you sure?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                InstanceManager.DeleteInstance(SelectedInstance!);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            InstanceManager.Save();
        }

        private void AddInstance(object sender, RoutedEventArgs e)
        {
            NewInstanceWindow window = new();
            window.Show();
        }

        public void UpdateInstanceList()
        {
            Dispatcher.Invoke(() =>
            {
                InstanceList.Items.Clear();

                if (SelectedInstance != null && !InstanceManager.Instances.Contains(SelectedInstance))
                    SelectedInstance = null;
            
                foreach (Instance i in InstanceManager.Instances)
                {
                    InstanceListViewItem listViewItem = new(i);
                    InstanceList.Items.Add(listViewItem);

                    if (i == SelectedInstance)
                        InstanceList.SelectedItem = listViewItem;
                }
            });
        }

        private void RunCurrentInstance(object sender, RoutedEventArgs e)
        {
            InstanceManager.RunInstance(SelectedInstance!);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedInstance = ((InstanceListViewItem) InstanceList.SelectedItem).Instance;
            InstanceName.Text = SelectedInstance.Name;
            InstanceVersion.Text = SelectedInstance.Version.ToString();
            // TODO: Fix the icon loading preventing the deletion of the instance because the icon is still loaded
            InstanceIconLarge.Source = new BitmapImage(new(Path.Join(SelectedInstance.Path, "icon.ico")));
        }

        public void SetStatus(string s)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = s;
            });
        }
    }
}