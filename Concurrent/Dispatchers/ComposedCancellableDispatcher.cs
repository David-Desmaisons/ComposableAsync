using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Dispatchers
{
    internal class ComposedCancellableDispatcher : ICancellableDispatcher
    {
        private readonly ICancellableDispatcher _First;
        private readonly ICancellableDispatcher _Second;

        public ComposedCancellableDispatcher(ICancellableDispatcher first, ICancellableDispatcher second)
        {
            _First = first;
            _Second = second;
        }

        public void Dispatch(Action action)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Action action)
        {
            throw new NotImplementedException();
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
