using System;
using System.IO;
using Newtonsoft.Json;

namespace MindustryLauncher.Avalonia;

public abstract class ServerInstance : Instance
{
    [field: NonSerialized][JsonIgnore]
    public StreamWriter? ServerInput { get; protected set; }

    [field: NonSerialized][JsonIgnore]
    public StreamReader? ServerOutput { get; protected set; }
}