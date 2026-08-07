using System;

namespace MindustryLauncher.Avalonia;

public class Observer<T>(Action<T> del) : IObserver<T>
{
    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(T value)
    {
        del.Invoke(value);
    }
}