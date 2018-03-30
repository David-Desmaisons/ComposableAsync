using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.WorkItems
{
    public class AsyncWorkItem<T> : IWorkItem
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<Task<T>> _Do;
        private bool _Cancelled = false;

        public AsyncWorkItem(Func<Task<T>> @do)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
        }

        public AsyncWorkItem(Func<Task<T>> @do, CancellationToken cancellationToken)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
            cancellationToken.Register(Cancel);
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
            _Cancelled = true;
        }

        public async void Do()
        {
            if (_Cancelled)
                return;

            try
            {
                _Source.TrySetResult(await _Do());
            }
            catch(Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }
}
