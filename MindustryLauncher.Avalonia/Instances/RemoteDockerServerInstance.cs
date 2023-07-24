using System;
using System.IO;
using System.Text;
using System.Threading;
using Renci.SshNet;

namespace MindustryLauncher.Avalonia.Instances;

public class RemoteDockerServerInstance : ServerInstance
{
    public string Ip { get; set; }
    public string Usernsame { get; set; }
    public string Password { get; set; }
    public string ContainerName { get; set; }

    [NonSerialized]
    private SshClient? client;
    public RemoteDockerServerInstance()
    {
    }

    public override void Run()
    {
        client = new(Ip, Usernsame, Password);
        
        client.Connect();

        MemoryStream inputStream = new();
        MemoryStream outputStream = new();
        
        
        
        
        Shell shell = client.CreateShell(inputStream, outputStream, outputStream);
        shell.Start();
        //Stream shell = client.CreateShellStream("bash", 80, 50, 1024, 1024, 1024);

        Thread.Sleep(100);

        ServerInput = new(inputStream, Encoding.UTF8);
        ServerInput.AutoFlush = true;

        ServerOutput = new(outputStream, Encoding.UTF8);
        
        /*
        ServerInput = new(shell, Encoding.UTF8);
        ServerInput.AutoFlush = true;
        ServerOutput = new(shell, Encoding.UTF8);
        */
        
        ServerInput.Write((char)13);
        Thread.Sleep(50);
        
        // attach the shell to the mindustry instance
        ServerInput.WriteLine($"docker start {ContainerName}");

        
        Thread.Sleep(50);

        
        ServerInput.WriteLine($"docker attach {ContainerName}");
        
        Thread.Sleep(50);
        //while (ServerOutput.EndOfStream);
        outputStream.Position = 0;
        string firstOutput = ServerOutput.ReadLine()!;
        
        IsRunning = true;
    }

    public override void Kill()
    {
        try
        {
            ServerInput?.WriteLine("exit");
            client?.Disconnect();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}