using System;
using System.Collections.Generic;
using System.Threading;

namespace ComposableAsync.Concurrent.Collections
{
    /// <summary>
    /// Spinning implementation of see cref="IMpScQueue<typeparam name="T"></typeparam>"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpinningMpscQueue<T> : IMpScQueue<T> where T: class
    {
        private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();
        private Node<T> _Head;
        private Node<T> _Tail;

        private CancellationToken CancellationToken => _CancellationTokenSource.Token;

        /// <summary>
        /// Create an empty instance of the queue
        /// </summary>
        public SpinningMpscQueue()
        {
            var empty = new Node<T>();
            _Head = empty;
            _Tail = empty;
        }

        /// <inheritdoc />
        public void Enqueue(T item) 
        {
            CancellationToken.ThrowIfCancellationRequested();
            
            var newItem = new Node<T>(item);
            var prev = Interlocked.Exchange(ref _Head, newItem);
            prev.Next = newItem;
        }

        /// <inheritdoc />
        public void CompleteAdding()
        {
            _CancellationTokenSource.Cancel();       
        }

        /// <inheritdoc />
        public IEnumerable<T> GetUnsafeQueue()
        {
            var node = _Tail.Next;
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }

        /// <inheritdoc />
        public void OnElements(Action<T> onItem) 
        {
            while (true)
            {
                SpinWait.SpinUntil(() => _Tail.Next != null || CancellationToken.IsCancellationRequested);
                CancellationToken.ThrowIfCancellationRequested();
                _Tail = _Tail.Next;
                onItem(_Tail.Value);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
