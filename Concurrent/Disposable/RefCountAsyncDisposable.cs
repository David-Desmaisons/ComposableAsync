using System;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class RefCountAsyncDisposable : IAsyncDisposable
    {
        private int _Count = 1;
        private IAsyncDisposable _AsyncDisposable;
        private readonly object _Locker = new object();
        private bool _AlreadyReleased = false;

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

        public Task DisposeAsync()
        {
            IAsyncDisposable disposable;
            lock (_Locker)
            {
                if (_AlreadyReleased)
                    return TaskBuilder.Completed;

                _AlreadyReleased = true;
                _Count--;
                if (_Count > 0)
                    return TaskBuilder.Completed;

                disposable = _AsyncDisposable;
                _AsyncDisposable = null;
            }

            return disposable?.DisposeAsync() ?? TaskBuilder.Completed;
        }

        private Task Release()
        {
            IAsyncDisposable disposable;
            lock (_Locker)
            {
                _Count--;
                if (_Count > 0)
                    return TaskBuilder.Completed;

                disposable = _AsyncDisposable;
                _AsyncDisposable = null;
            }

            return disposable?.DisposeAsync() ?? TaskBuilder.Completed;
        }
    }
}
