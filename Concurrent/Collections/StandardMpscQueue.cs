using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Concurrent.Collections
{
    public class StandardMpscQueue<T> : IMpScQueue<T> where T: class
    {
        private readonly BlockingCollection<T> _Queue = new BlockingCollection<T>();
        private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

        public void Enqueue(T item) 
        {
            _Queue.Add(item);   
        }

        public void CompleteAdding()
        {
            _CancellationTokenSource.Cancel();
            _Queue.CompleteAdding();
        }

        public IEnumerable<T> GetUnsafeQueue() => _Queue;

        public void OnElements(Action<T> onItem) 
        {
            foreach (var item in _Queue.GetConsumingEnumerable(_CancellationTokenSource.Token))
            {
                onItem(item);
            }
        }

        public void Dispose()
        {
            _Queue.Dispose();
        }
    }
}
