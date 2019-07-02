using System;
using System.Collections.Generic;
using System.Threading;
using ComposableAsync.Concurrent.Signals;

namespace ComposableAsync.Concurrent.Collections
{
    /// <summary>
    /// Blocking implementation of IMpScQueue
    /// Inspired by and adapted from http://www.1024cores.net/
    /// Ref: http://www.1024cores.net/home/lock-free-algorithms/queues/non-intrusive-mpsc-node-based-queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockingMpscQueue<T> : IMpScQueue<T> where T: class
    {
        private readonly SimplifiedEventCount _SimplifiedEventCount = new SimplifiedEventCount();
        private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _CancellationToken;

        private Node<T> _Head;
        private Node<T> _Tail;
        
        /// <summary>
        /// Create an empty instance of the queue
        /// </summary>
        public BlockingMpscQueue()
        {
            var empty = new Node<T>();
            _Head = empty;
            _Tail = empty;
            _CancellationToken = _CancellationTokenSource.Token;
        }

        /// <inheritdoc />
        public void Enqueue(T item)
        {
            _CancellationToken.ThrowIfCancellationRequested();

            var newItem = new Node<T>(item);
            var prev = Interlocked.Exchange(ref _Head, newItem);
            prev.Next = newItem;

            _SimplifiedEventCount.NotifyOne();
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
                var value = Next();
                onItem(value);
            }
        }

        private T Next()
        {
            var spin = new SpinWait();
            while (_Tail.Next ==null && !spin.NextSpinWillYield)
            {
                spin.SpinOnce();
            }

            if (_Tail.Next != null)
            {
                _Tail = _Tail.Next;
                return _Tail.Value;
            }

            while (true)
            {
                _SimplifiedEventCount.PrepareWait();

                if (_Tail.Next != null) 
                {
                    _SimplifiedEventCount.RetireWait();
                    _Tail = _Tail.Next;
                    return _Tail.Value;
                }

                _SimplifiedEventCount.Wait(_CancellationToken);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _CancellationTokenSource.Dispose();
            _SimplifiedEventCount.Dispose();
        }
    }
}
