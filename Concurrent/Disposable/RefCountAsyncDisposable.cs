using System;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class RefCountAsyncDisposable: IShareableAsyncDisposable
    {
        private int _Count = 0;
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
