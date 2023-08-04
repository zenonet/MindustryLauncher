using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Renci.SshNet;

namespace MindustryLauncher.Avalonia.Instances;

public class RemoteDockerServerInstance : ServerInstance
{
    public string Ip { get; set; }
    public string Usernsame { get; set; }
    public string Password { get; set; }
    public string ContainerName { get; set; }

    [NonSerialized]
    [JsonIgnore]
    private SshClient? client;

    public RemoteDockerServerInstance()
    {
    }

    public override void Run()
    {
        client = new(Ip, Usernsame, Password);

        client.Connect();

        client.RunCommand($"docker restart {ContainerName}");

        var shell = this.client.CreateShellStream("", 0, 0, 0, 0, 4096);
        //Stream shell = client.CreateShellStream("bash", 80, 50, 1024, 1024, 1024);

        ServerInput = new(shell, Encoding.UTF8);
        ServerInput.AutoFlush = true;
        ServerOutput = new(shell, Encoding.UTF8);
        
        ServerInput.Write((char)13);

        ServerInput.WriteLine($"docker attach {ContainerName}");
        Thread.Sleep(200);

        string lastLine;
        do
        {
            lastLine = ServerOutput.ReadLine()!;
        } while (!ServerOutput.EndOfStream);

        // This means that the server probably exited
        if (lastLine.Contains($"{Usernsame}@"))
        {
            OnInstanceExited(1);
            return;
        }
        
        IsRunning = true;
        OnInstanceStarted();
    }

    public override void Kill()
    {
        try
        {
            // stop the server
            client!.RunCommand("save mindustryLauncherAutoSave");
            client.RunCommand("stop");
            client.RunCommand("exit");
            client.Disconnect();
            IsRunning = false;
            OnInstanceExited(0);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}