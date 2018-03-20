using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class AsyncDisposable : IAsyncDisposable
    {
        private IDisposable _Disposable;
        public AsyncDisposable(IDisposable disposable)
        {
            _Disposable = disposable;
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref _Disposable, null);
            disposable.Dispose();
        }

        public Task DisposeAsync()
        {
            Dispose();
            return TaskBuilder.Completed;
        }
    }
}
