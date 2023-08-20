namespace MindustryLauncher.Avalonia;

public interface ILocalInstance
{
    public string Path { get; }
    public string JarPath { get; }
    public string JarHash { get; set; }
}