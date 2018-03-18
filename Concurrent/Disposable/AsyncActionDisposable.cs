using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class AsyncActionDisposable : IAsyncDisposable
    {
        private Func<Task> _Dispose;
        public AsyncActionDisposable(Func<Task> dispose)
        {
            _Dispose = dispose;
        }

        public void Dispose() => DisposeAsync().Wait();

        public Task DisposeAsync()
        {
            var dispose = Interlocked.Exchange(ref _Dispose, null);
            return dispose?.Invoke()?? TaskBuilder.Completed;
        }
    }
}
