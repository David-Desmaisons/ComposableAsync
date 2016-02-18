using System;

namespace EasyActor.Pipeline
{
    public interface IClosedPipeline<T> : IConsumer<T>
    {
        IDisposable Connect(IObservable<T> source);
    }
}
