using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.WorkItems
{
    public class AsyncWorkItem<T> : IWorkItem
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<Task<T>> _Do;
        private readonly CancellationToken _CancellationToken;
        private CancellationTokenRegistration _CancellationTokenRegistration;

        public AsyncWorkItem(Func<Task<T>> @do)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
            _CancellationToken = CancellationToken.None;
        }

        public AsyncWorkItem(Func<Task<T>> @do, CancellationToken cancellationToken)
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

        public async void Do()
        {
            if (_CancellationToken.IsCancellationRequested)
                return;

            _CancellationTokenRegistration.Dispose();

            try
            {
                _Source.TrySetResult(await _Do());
            }
            catch (OperationCanceledException operationCanceledException)
            {
                if ((_CancellationToken.IsCancellationRequested) && (operationCanceledException.CancellationToken == _CancellationToken))
                    _Source.TrySetCanceled();
                else
                    _Source.TrySetException(operationCanceledException);
            }
            catch (Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }
}
