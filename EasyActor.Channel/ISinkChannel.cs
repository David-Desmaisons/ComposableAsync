using System;

namespace EasyActor.Channel
{
    public interface ISinkChannel<T>: IObservableChannel<T>, IObserver<T>, IDisposable
    {
    }
}
