using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class RefCountAsyncDisposable
    {
        private static readonly ConditionalWeakTable<IAsyncDisposable, RefCountAsyncDisposable> _WeakTable =
            new ConditionalWeakTable<IAsyncDisposable, RefCountAsyncDisposable>();
        private static readonly object _StaticLocker = new object();

        private int _Count = 0;
        private IAsyncDisposable _AsyncDisposable;

        private RefCountAsyncDisposable(IAsyncDisposable asyncDisposable)
        {
            _AsyncDisposable = asyncDisposable;
        }

        public static IAsyncDisposable Using(IAsyncDisposable asyncDisposable)
        {
            lock (_StaticLocker)
            {
                var refCount = _WeakTable.GetValue(asyncDisposable, d => new RefCountAsyncDisposable(d));
                return refCount.GetDisposable();
            }
        }


        private IAsyncDisposable GetDisposable()
        {
            if (_AsyncDisposable == null)
                return NullAsyncDisposable.Null;

            _Count++;
            return new AsyncActionDisposable(Release);
        }

        private Task Release()
        {
            IAsyncDisposable disposable;
            lock (_StaticLocker)
            {
                _Count--;
                if (_Count > 0)
                    return TaskBuilder.Completed;

                disposable = _AsyncDisposable;
                _AsyncDisposable = null;
                _WeakTable.Remove(disposable);
            }

            return disposable?.DisposeAsync() ?? TaskBuilder.Completed;
        }
    }
}
