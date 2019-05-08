using System;

namespace Concurrent.Channel
{
    /// <summary>
    /// InChannel interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInChannel<in T>: IObserver<T>, IDisposable
    {
    }
}
