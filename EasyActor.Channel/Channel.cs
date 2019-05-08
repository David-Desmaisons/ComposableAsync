using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

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

        public IOutChannel<TNew> Transform<TNew>(Func<T, TNew> transform)
        {
            return new Channel<TNew>(Observable.Select(transform));
        }
    }
}
