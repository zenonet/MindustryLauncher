using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using MindustryLauncher.Avalonia.ServerManagementGui;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace MindustryLauncher.Avalonia.Windows;

public partial class ServerManagementWindow : Window
{
    public ServerInstance Server;

    private Task ServerHandlerTask;

    private ObservableCollection<PlayerControl> playerControls = new();

    public ServerManagementWindow(ServerInstance server)
    {
        Server = server;

        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        StartStopButton.Content = Server.IsRunning ? "Stop" : "Start";

        PlayerList.ItemsSource = playerControls;

        StartStopButton.Click += OnStartStopClick;
        HostButton.Click += OnHostButtonClick;
        ConsoleSendButton.Click += SendConsoleCommand;

        ServerHandlerTask = Task.Run(ServerHandler);
    }

    private void SendConsoleCommand(object? sender, RoutedEventArgs e)
    {
        try { EnsureServerIsRunning(); }
        catch {return;}
        

        Server.ServerInput?.WriteLine(ConsoleInput.Text);
        ConsoleInput.Text = "";
    }

    private void EnsureServerIsRunning()
    {
        IMsBox<ButtonResult>? confirmationBox = MessageBoxManager.GetMessageBoxStandard(new()
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentTitle = "Server is not running",
            ContentMessage = "You can't do that because shis server is not running.",
            Topmost = true,
        });
        confirmationBox.ShowAsPopupAsync(this);
        throw new ServerNotRunningException();
    }

    private void ServerHandler()
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (true)
        {
            if (!Server.IsRunning)
                continue;

            /*
            if (sw.ElapsedMilliseconds > 1000)
            {
                sw.Restart();
                // Update player list
                Server.ServerInput!.WriteLine("players");
                Task.Delay(150);

                string rawPlayerList = "";
                while (Server.ServerOutput!.Peek() != -1)
                {
                    rawPlayerList += (char)Server.ServerOutput.Read();
                }

                GeneratePlayerList(rawPlayerList);
            }*/

            string? line = Server.ServerOutput!.ReadLine();
            if (line == null)
                continue;


            Match match = Regex.Match(line, "\\[.*\\] \\[I\\] (.*?) has connected\\. \\[(\\w*==)\\]");
            if (match.Success)
            {
                Match matchCopy = match;
                Dispatcher.UIThread.InvokeAsync(() => { playerControls.Add(new(matchCopy.Groups[1].Value, matchCopy.Groups[2].Value)); });
            }

            match = Regex.Match(line, "\\[.*\\] \\[I\\] (.*?) has disconnected\\. \\[(\\w*==)\\].*");
            if (match.Success)
            {
                Dispatcher.UIThread.InvokeAsync(() => { playerControls.Remove(playerControls.First(x => x.Uuid == match.Groups[2].Value)); });
            }


            Dispatcher.UIThread.Invoke(() =>
            {
                ConsoleOutput.Text += $"\n{line}";
                ConsoleOutput.CaretIndex = int.MaxValue;
            });
        }
    }

    private void GeneratePlayerList(string rawPlayerList)
    {
        MatchCollection matches = Regex.Matches(rawPlayerList, "\\[.*\\] (.*?) \\/ ID: (.*==) \\/ IP: (.*)");

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            playerControls.Clear();
            foreach (Match match in matches)
            {
                playerControls.Add(new(match.Groups[1].Value, match.Groups[2].Value));
            }
        });
    }


    private void OnHostButtonClick(object? sender, RoutedEventArgs e)
    {
        try { EnsureServerIsRunning(); }
        catch {return;}        
        
        Server.ServerInput!.WriteLine("host");
    }

    private void OnStartStopClick(object? sender, RoutedEventArgs e)
    {
        if (Server.IsRunning)
        {
            Server.Kill();
            StartStopButton.Content = "Start";
        }
        else
        {
            Server.Run();
            StartStopButton.Content = "Stop";
        }
    }

    internal class ServerNotRunningException : Exception
    {
    }
}