using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal class CancellableWorkItem<T> : ITraceableWorkItem<T>
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<T> _Do;
        private readonly CancellationToken _CancellationToken;
        private CancellationTokenRegistration _CancellationTokenRegistration;

        internal CancellableWorkItem(Func<T> @do, CancellationToken cancellationToken)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
            _CancellationToken = cancellationToken;
            _CancellationTokenRegistration = cancellationToken.Register(Cancel);
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
        }

        public void Do()
        {
            if (_CancellationToken.IsCancellationRequested)
                return;

            _CancellationTokenRegistration.Dispose();

            try
            {
                _Source.TrySetResult(_Do());
            }
            catch (Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }
}
