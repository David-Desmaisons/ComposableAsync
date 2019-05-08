using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Concurrent.Channel
{
    public class OutChannel<T> : IOutChannel<T>
    {
        protected readonly IObservable<T> Observable;
        public OutChannel(IObservable<T> observable)
        {
            Observable = observable;
        }

        public IOutChannel<TNew> Transform<TNew>(Func<IObservable<T>, IObservable<TNew>> transform)
        {
            return new OutChannel<TNew>(transform(Observable));
        }

        public IAsyncEnumerator<T> GetMessages()
        {
            return Observable.ToAsyncEnumerable().GetEnumerator();
        }

        public IOutChannel<TNew> Transform<TNew>(Func<T, TNew> transform)
        {
            return new OutChannel<TNew>(Observable.Select(transform));
        }
    }
}
