using System;

namespace MindustryLauncher.Avalonia;

public abstract class Instance
{
    public Version Version { get; set; }
    
    public string Name { get; set; }

    public virtual void Run()
    {
        
    }

    public virtual void Kill()
    {
    }
    
    public virtual void DeleteInstance()
    {

    }
    
    public virtual bool IsRunning { get; protected set; }

    #region Events

    

    public event EventHandler InstanceStarted = delegate { };
    public event EventHandler<int> InstanceExited = delegate { };

    protected void OnInstanceStarted()
    {
        InstanceStarted.Invoke(this, EventArgs.Empty);
    }

    protected void OnInstanceExited(int exitCode)
    {
        InstanceExited.Invoke(this, exitCode);
    }
    
    #endregion
}