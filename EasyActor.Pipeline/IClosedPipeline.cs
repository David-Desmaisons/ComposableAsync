using System;
using EasyActor.Disposable;

namespace EasyActor.Pipeline
{
    public interface IClosedPipeline<T> : IConsumer<T>, IAsyncDisposable
    {
        IDisposable Connect(IObservable<T> source);
    }
}
