using System;
using System.IO;

namespace MindustryLauncher.Avalonia;

public abstract class ServerInstance : Instance
{
    [field: NonSerialized]
    public StreamWriter? ServerInput { get; protected set; }

    [field: NonSerialized]
    public StreamReader? ServerOutput { get; protected set; }
}