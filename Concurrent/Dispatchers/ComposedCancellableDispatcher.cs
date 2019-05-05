using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Dispatchers
{
    internal sealed class ComposedCancellableDispatcher : ComposedDispatcher, ICancellableDisposableDispatcher
    {
        private readonly ICancellableDispatcher _First;
        private readonly ICancellableDispatcher _Second;

        public ComposedCancellableDispatcher(ICancellableDispatcher first, ICancellableDispatcher second) :
            base(first, second)
        {
            _First = first;
            _Second = second;
        }

        public Task DisposeAsync()
        {
            return Task.WhenAll(DisposeAsync(_First), DisposeAsync(_Second));
        }

        private static Task DisposeAsync(ICancellableDispatcher disposable) => (disposable as IAsyncDisposable)?.DisposeAsync() ?? Task.CompletedTask;

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
