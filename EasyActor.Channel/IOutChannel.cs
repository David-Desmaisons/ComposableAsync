using System;
using System.Collections.Generic;

namespace EasyActor.Channel
{
    public interface IOutChannel<out T>
    {
        IAsyncEnumerator<T> GetMessages();

        IOutChannel<TNew> Transform<TNew>(Func<IObservable<T>, IObservable<TNew>> transform);

        IOutChannel<TNew> Transform<TNew>(Func<T, TNew> transform);
    }
}
