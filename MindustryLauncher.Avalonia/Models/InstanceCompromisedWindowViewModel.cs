using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MindustryLauncher.Avalonia.Models;

public partial class InstanceCompromisedWindowViewModel : ObservableObject
{
    public InstanceCompromisedWindowViewModel(List<Instance> compromisedInstances)
    {
        CompromisedInstances = new(compromisedInstances);
        CompromisedInstances.CollectionChanged += (sender, args) =>
        {
            if (args.NewItems == null || args.NewItems.Count == 0)
            {
                OnWindowCloseRequested?.Invoke();
            }
        };
    }

    public event Action OnWindowCloseRequested;

    public ObservableCollection<Instance> CompromisedInstances { get; set; }

    public string Title => CompromisedInstances.Count > 1 ? "Multiple mindustry jar files appear to be compromised" : "A mindustry jar file appears to be compromised";

    [ObservableProperty]
    private bool showMore;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnyInstanceSelected))]
    private Instance? selectedInstance;

    public bool IsAnyInstanceSelected => SelectedInstance != null;
    
    [RelayCommand]
    public void ShowMoreOrLess()
    {
        ShowMore = !ShowMore;
    }

    [RelayCommand]
    public void Dismiss()
    {
        CompromisedInstances.Remove(SelectedInstance);
    }

    [RelayCommand]
    public void DismissAll()
    {
        CompromisedInstances.Clear();
    }

    [RelayCommand]
    public void AuthorizeThisChange()
    {
        UpdateInstanceHash((ILocalInstance) SelectedInstance);
        CompromisedInstances.Remove(SelectedInstance);
    }

    [RelayCommand]
    public void AuthorizeAllChanges()
    {
        foreach (Instance i in CompromisedInstances)
        {
            UpdateInstanceHash((ILocalInstance) i);
        }
        CompromisedInstances.Clear();
    }

    private static void UpdateInstanceHash(ILocalInstance i)
    {
        i.JarHash = Utils.GetSha256OfFile(i.JarPath);
    } 
}