using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MindustryLauncher.Avalonia;
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
            OpenMindustryFolderButton.Click += OpenMindustryFolder;
        }

        private void OpenMindustryFolder(object? sender, RoutedEventArgs e)
        {
            if (SelectedInstance is not IFolderOpenable instance)
                return;

            string mindustryPath = Path.Join(instance.Path, "Mindustry");

            using Process folderOpener = new();
            folderOpener.StartInfo.FileName = Path.GetDirectoryName(mindustryPath);
            folderOpener.StartInfo.UseShellExecute = true;
            folderOpener.Start();
        }

        private void DeleteInstance(object? sender, RoutedEventArgs e)
        {
            // Buffer the selected instance in case another is seleted
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
            //TODO:
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
                // TODO: Fix the icon loading preventing the deletion of the instance because the icon is still loaded
                InstanceIconLarge.Source = new Bitmap(Path.Join(instance.Path, "icon.ico"));
            }

            // Update the text on the run button according to the instances status
            SelectedInstance.InstanceStarted += SetStopButtonText;
            SelectedInstance.InstanceExited += SetRunButtonText;

            if (SelectedInstance.IsRunning)
                SetStopButtonText();
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