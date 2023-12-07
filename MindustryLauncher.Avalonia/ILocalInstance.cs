namespace MindustryLauncher.Avalonia;

public interface ILocalInstance
{
    public string Path { get; }
    public string JarPath { get; }
    public string JarHash { get; set; }
    
    public bool VerifyIntegrity(ILocalInstance instance)
    {
        string jarPath = instance.JarPath;
        string jarHash = Utils.GetSha256OfFile(jarPath);
        return jarHash == instance.JarHash;
    }
}