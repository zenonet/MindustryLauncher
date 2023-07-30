﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace MindustryLauncher.Avalonia.Setup;

public partial class SetupWindow : Window
{
    public SetupWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
            Carousel.SelectedIndex = 0;
        NextPageButton.Click += OnNextPageButtonClicked;
        JavaFromCustomPathRadioButton.IsCheckedChanged += OnJavaFromCustomPathRadioButtonIsCheckedChanged;
        JavaChooseFileButton.Click += OnJavaChooseFileButtonClicked;
    }

    private async void OnJavaChooseFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new();
        options.Title = "Select Java Runtime executable";
        var files = await this.StorageProvider.OpenFilePickerAsync(options);

        if (files.Count != 1)
            return;

        JavaPathTextBox.Text = files[0].Path.AbsolutePath;
    }

    private void OnJavaFromCustomPathRadioButtonIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        JavaPathTextBox.IsEnabled = JavaFromCustomPathRadioButton.IsChecked ?? false;
        JavaChooseFileButton.IsEnabled = JavaFromCustomPathRadioButton.IsChecked ?? false;
    }

    private void OnNextPageButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (Carousel.SelectedIndex == Carousel.ItemCount - 1)
        {
            OnFinishButtonClicked();
            return;
        }

        Carousel.Next();
        switch (Carousel.SelectedIndex)
        {
            case 1:
                NextPageButton.IsEnabled = false;
                StartCachingVersions();
                break;
            case 2:
                InitializeJavaSelectionPage();
                break;
        }

        if (Carousel.SelectedIndex == Carousel.ItemCount - 1)
        {
            NextPageButton.Content = "Finish";
        }
    }

    private void InitializeJavaSelectionPage()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "java",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        try
        {
            Process.Start(startInfo);
        }
        catch (Exception)
        {
            JavaFromPathRadioButton.IsEnabled = false;
            JavaFromPathRadioButton.Content += " [There is no Java Runtime in PATH]";
            JavaFromCustomPathRadioButton.IsChecked = true;
        }
    }

    private void StartCachingVersions()
    {
        // Get version count to display progress
        int totalVersionCount;
        int gottenVersionCount = 0;

        void HandleProgressUpdateOnUiThread()
        {
            CachingProgressBar.Value = gottenVersionCount / (float) totalVersionCount * 100;

            if (gottenVersionCount == totalVersionCount)
                NextPageButton.IsEnabled = true;
        }

        Task.Run(() =>
        {
            totalVersionCount = MindustryDownloader.GetVersionCount();
            VersionCache.CacheVersions(onProgressChanged: versions =>
            {
                gottenVersionCount += versions.Length;

                Dispatcher.UIThread.InvokeAsync(HandleProgressUpdateOnUiThread);
            });
        });
    }


    void OnFinishButtonClicked()
    {
        DataManager.Save();

        // Open the actual launcher and close the setup window
        var lifetime =
            (IClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!;
        lifetime.MainWindow = new MainWindow();
        lifetime.MainWindow.Show();
        this.Close();
    }
}