using System;
using System.Threading.Tasks;

namespace EasyActor.Disposable
{
    public class AsyncDisposable: IAsyncDisposable
    {
        private readonly Func<Task> _Dispose;
        public AsyncDisposable(Func<Task> dispose)
        {
            _Dispose = dispose;
        }

        public void Dispose() => _Dispose().Wait();

        public Task DisposeAsync() => _Dispose();
    }
}
