using System;
using System.Threading.Tasks;

namespace Concurrent.Disposable
{
    public class AsyncActionDisposable : IAsyncDisposable
    {
        private readonly Func<Task> _Dispose;
        public AsyncActionDisposable(Func<Task> dispose)
        {
            _Dispose = dispose;
        }

        public void Dispose() => _Dispose().Wait();

        public Task DisposeAsync() => _Dispose();
    }
}
