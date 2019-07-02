using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ComposableAsync.Concurrent.Collections
{
    /// <summary>
    /// Implementation of IMpScQueue relying on .Net BlockingCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StandardMpscQueue<T> : IMpScQueue<T> where T: class
    {
        private readonly BlockingCollection<T> _Queue = new BlockingCollection<T>();
        private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

        /// <inheritdoc />
        public void Enqueue(T item) 
        {
            _Queue.Add(item);   
        }

        /// <inheritdoc />
        public void CompleteAdding()
        {
            _CancellationTokenSource.Cancel();
            _Queue.CompleteAdding();
        }

        /// <inheritdoc />
        public IEnumerable<T> GetUnsafeQueue() => _Queue;

        /// <inheritdoc />
        public void OnElements(Action<T> onItem) 
        {
            foreach (var item in _Queue.GetConsumingEnumerable(_CancellationTokenSource.Token))
            {
                onItem(item);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Queue.Dispose();
        }
    }
}
