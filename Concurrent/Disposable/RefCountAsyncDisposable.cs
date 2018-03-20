using System;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class RefCountAsyncDisposable : IAsyncDisposable
    {
        private int _Count = 0;
        private bool _PrimaryDisposed = false;
        private IAsyncDisposable _AsyncDisposable;
        private readonly object _Locker = new object();
        public RefCountAsyncDisposable(IAsyncDisposable asyncDisposable)
        {
            _AsyncDisposable = asyncDisposable;
        }

        public IAsyncDisposable GetDisposable()
        {
            lock (_Locker)
            {
                if (_AsyncDisposable == null)
                    return NullAsyncDisposable.Null;

                _Count++;
                return new AsyncActionDisposable(Release);
            }
        }

        private Task Release()
        {
            return AltereState(d => { d._Count--; });
        }

        public Task DisposeAsync()
        {
            return AltereState(d => d._PrimaryDisposed = true);
        }

        private Task AltereState(Action<RefCountAsyncDisposable> update)
        {
            IAsyncDisposable disposable;
            lock (_Locker)
            {
                update(this);
                if ((_Count > 0) || (!_PrimaryDisposed))
                    return TaskBuilder.Completed;

                disposable = _AsyncDisposable;
                _AsyncDisposable = null;
            }

            return disposable?.DisposeAsync() ?? TaskBuilder.Completed;
        }

        public void Dispose()
        {
            DisposeAsync().Wait();
        }
    }
}
