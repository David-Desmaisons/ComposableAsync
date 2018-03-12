using System;

namespace EasyActor.Channel
{
    public interface IInChannel<in T>: IObserver<T>, IDisposable
    {
    }
}
