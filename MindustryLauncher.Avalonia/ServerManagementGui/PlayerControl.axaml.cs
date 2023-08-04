using Avalonia.Controls;
using MindustryLauncher.Avalonia.Models;


namespace MindustryLauncher.Avalonia.ServerManagementGui;

public partial class PlayerControl : UserControl
{
    public PlayerModel Data => (PlayerModel) DataContext!;
    public PlayerControl(PlayerModel playerModel)
    {
        DataContext = playerModel;
        InitializeComponent();
    }
}