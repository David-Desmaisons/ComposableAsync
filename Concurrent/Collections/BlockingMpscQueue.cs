using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Concurrent.Signals;

//Inspired by and adapted from http://www.1024cores.net/
//Ref: http://www.1024cores.net/home/lock-free-algorithms/queues/non-intrusive-mpsc-node-based-queue

namespace Concurrent.Collections
{
    public class BlockingMpscQueue<T> : IMpScQueue<T> where T: class
    {
        private readonly SimplifiedEventCount _SimplifiedEventCount = new SimplifiedEventCount();
        private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

        private Node<T> _Head;
        private Node<T> _Tail;
        
        public BlockingMpscQueue()
        {
            var empty = new Node<T>();
            _Head = empty;
            _Tail = empty;
        }

        public void Enqueue(T item)
        {
            ThrowIfCanceled();

            var newItem = new Node<T>(item);
            var prev = Interlocked.Exchange(ref _Head, newItem);
            prev.Next = newItem;

            _SimplifiedEventCount.NotifyOne();
        }

        public void CompleteAdding()
        {
            _CancellationTokenSource.Cancel();
        }

        public IEnumerable<T> GetUnsafeQueue()
        {
            var node = _Tail.Next;
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }

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

                _SimplifiedEventCount.Wait(_CancellationTokenSource.Token);
                ThrowIfCanceled();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfCanceled()
        {
            _CancellationTokenSource.Token.ThrowIfCancellationRequested();
        }

        public void Dispose()
        {
            _CancellationTokenSource.Dispose();
            _SimplifiedEventCount.Dispose();
        }
    }
}
