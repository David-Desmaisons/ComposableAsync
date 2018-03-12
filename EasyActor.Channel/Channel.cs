using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyActor.Channel
{
    public class Channel<T> : IOutChannel<T>
    {
        protected readonly IObservable<T> Observable;
        public Channel(IObservable<T> observable)
        {
            Observable = observable;
        }

        public IOutChannel<TNew> Transform<TNew>(Func<IObservable<T>, IObservable<TNew>> transform)
        {
            return new Channel<TNew>(transform(Observable));
        }

        public IAsyncEnumerator<T> GetMessages()
        {
            return Observable.ToAsyncEnumerable().GetEnumerator();
        }
    }
}
