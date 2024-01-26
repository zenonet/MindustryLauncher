using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace MindustryLauncher.Avalonia.UserControls;

public partial class MindustryVersionComboBox : UserControl
{
    private ObservableCollection<ComboBoxItem> versionsInDropdown = new();

    private bool AllowShowingMoreVersions { get; set; } = true;

    public Version? SelectedVersion =>
        Version.Parse(((ComboBoxItem) VersionDropDown.SelectedItem!).Content!.ToString()!);

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
        VersionDropDown.SelectedIndex = 0;
    }

    public async void LoadVersions()
    {
        await VersionCache.CacheVersions();
        Version[] versions = VersionCache.GetAllCachedVersions(); //MindustryDownloader.GetVersions(count);

        LoadFromVersionEnumerable(versions);

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
/*
        if (!AllowShowingMoreVersions || versionsInDropdown.Any(x => Version.Parse(x.Content.ToString()) == Version.MinVersionWithBuild))
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
        versionsInDropdown.Add(loadMoreButton);*/
    }
/*
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
    }*/
}