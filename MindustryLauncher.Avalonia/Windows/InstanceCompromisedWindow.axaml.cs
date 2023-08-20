using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MindustryLauncher.Avalonia.Models;

namespace MindustryLauncher.Avalonia.Windows;

public partial class InstanceCompromisedWindow : Window
{
    public InstanceCompromisedWindow(InstanceCompromisedWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OnWindowCloseRequested += Close;
    }
}