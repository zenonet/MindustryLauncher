using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MindustryLauncher.Avalonia.UserControls;

public partial class MindustryVersionComboBox : UserControl
{
    private ObservableCollection<ComboBoxItem> versionsInDropdown = new();

    private bool AllowShowingMoreVersions { get; set; } = true;

    public Version? SelectedVersion => Version.Parse(((ComboBoxItem) VersionDropDown.SelectedItem!).Content!.ToString()!);

    public MindustryVersionComboBox(IEnumerable<Version> versions)
    {
        InitializeComponent();
        AllowShowingMoreVersions = false;
        LoadFromVersionEnumerable(versions);

        Init();
    }

    private void Init()
    {
        VersionDropDown.ItemsSource = versionsInDropdown;
        /*
        VersionDropDown.SelectionChanged += (_, args) =>
        {
            if (ReferenceEquals(versionsInDropdown[^1], VersionDropDown.SelectedItem))
            {
                versionsInDropdown[^1].Focus();
                VersionDropDown.IsDropDownOpen = true;
            }
        };*/
    }

    public void LoadVersions(int count, bool clearOld = true)
    {
        AllowShowingMoreVersions = true;
        Version[] versions = MindustryDownloader.GetVersions(count);
        LoadFromVersionEnumerable(versions, clearOld);

        Init();
    }

    public MindustryVersionComboBox()
    {
        InitializeComponent();

        Init();
    }


    public void LoadFromVersionEnumerable(IEnumerable<Version> versions, bool clearOld = true)
    {
        if (clearOld)
            versionsInDropdown.Clear();

        foreach (Version v in versions)
        {
            versionsInDropdown.Add(new()
            {
                Content = v.ToString(),
            });
        }

        if (!AllowShowingMoreVersions)
            return;

        Button button = new()
        {
            Content = "Load More",
        };
        button.Click += OnLoadMore;
        ComboBoxItem loadMoreButton = new()
        {
            Content = button,
        };
        versionsInDropdown.Add(loadMoreButton);
    }

    private void OnLoadMore(object? sender, RoutedEventArgs routedEventArgs)
    {
        // Remove the more button
        versionsInDropdown.Remove(versionsInDropdown.First(x => x.Content is Button));

        Version smallestVersion = versionsInDropdown.Select(comboBoxItem =>
        {
            if (comboBoxItem.Content is Button)
                return new(int.MaxValue, int.MaxValue);

            return Version.Parse(comboBoxItem.Content!.ToString()!);
        }).Min();

        IEnumerable<Version> nextVersions = MindustryDownloader.GetVersionPage(smallestVersion);

        if (nextVersions.Min() < new Version(58, 0))
        {
            List<Version> nextVersionAsList = nextVersions.ToList();
            // Remove all version that don't have builds attached.
            nextVersionAsList.RemoveAll(x => x < new Version(58, 0));
            nextVersions = nextVersionAsList;
        }

        LoadFromVersionEnumerable(nextVersions, false);
    }
}