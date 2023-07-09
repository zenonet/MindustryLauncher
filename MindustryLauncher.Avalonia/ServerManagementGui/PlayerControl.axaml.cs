using Avalonia.Controls;


namespace MindustryLauncher.Avalonia.ServerManagementGui;

public partial class PlayerControl : UserControl
{
    public string PlayerName { get; }
    public string Uuid { get; }
    
    public PlayerControl(string playerName, string uuid)
    {
        PlayerName = playerName;
        Uuid = uuid;
        InitializeComponent();
        PlayerNameTextBlock.Text = playerName;
    }
}