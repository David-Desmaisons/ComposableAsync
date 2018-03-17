using System;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace EasyActor.Disposable
{
    public class AsyncDisposable : IAsyncDisposable
    {
        private readonly IDisposable _Disposable;
        public AsyncDisposable(IDisposable disposable)
        {
            _Disposable = disposable;
        }

        public void Dispose() => _Disposable.Dispose();

        public Task DisposeAsync()
        {
            Dispose();
            return TaskBuilder.Completed;
        }
    }
}
