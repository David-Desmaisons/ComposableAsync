using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class RefCountAsyncDisposable
    {
        private int _Count = 0;
        private readonly IAsyncDisposable _AsyncDisposable;
        public RefCountAsyncDisposable(IAsyncDisposable asyncDisposable)
        {
            _AsyncDisposable = asyncDisposable;
        }

        public IAsyncDisposable GetDisposable()
        {
            Interlocked.Increment(ref _Count);
            return new AsyncActionDisposable(Release);
        }

        private Task Release()
        {
            return Interlocked.Decrement(ref _Count) == 0 ? _AsyncDisposable.DisposeAsync() : TaskBuilder.Completed;
        }
    }
}
