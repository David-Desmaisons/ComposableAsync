using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Dispatchers
{
    internal sealed class ComposedCancellableDispatcher : ComposedDispatcher, ICancellableDispatcher
    {
        private readonly ICancellableDispatcher _First;
        private readonly ICancellableDispatcher _Second;

        public ComposedCancellableDispatcher(ICancellableDispatcher first, ICancellableDispatcher second):
            base(first, second)
        {
            _First = first;
            _Second = second;
        }

        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            await _First.Enqueue(() => _Second.Enqueue(action, cancellationToken), cancellationToken);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return await _First.Enqueue(() => _Second.Enqueue(action, cancellationToken), cancellationToken);
        }
    }
}
