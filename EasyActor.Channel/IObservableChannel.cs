using System;
using System.Collections.Generic;

namespace EasyActor.Channel
{
    public interface IObservableChannel<out T>
    {
        IAsyncEnumerator<T> GetMessages();

        IObservableChannel<TNew> Transform<TNew>(Func<IObservable<T>, IObservable<TNew>> transform);
    }
}
