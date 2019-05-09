using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Concurrent.Channel
{
    /// <summary>
    /// <see cref="IOutChannel&lt;T&gt;"/> implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OutChannel<T> : IOutChannel<T>
    {
        protected readonly IObservable<T> Observable;

        /// <summary>
        /// Instantiate a new OutChannel from an <see cref="IObservable&lt;T&gt;"/>
        /// </summary>
        public OutChannel(IObservable<T> observable)

        {
            Observable = observable;
        }

        /// <inheritdoc />
        public IOutChannel<TNew> Map<TNew>(Func<IObservable<T>, IObservable<TNew>> transform)
        {
            return new OutChannel<TNew>(transform(Observable));
        }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetMessages()
        {
            return Observable.ToAsyncEnumerable().GetEnumerator();
        }

        /// <inheritdoc />
        public IOutChannel<TNew> Map<TNew>(Func<T, TNew> transform)
        {
            return new OutChannel<TNew>(Observable.Select(transform));
        }
    }
}
