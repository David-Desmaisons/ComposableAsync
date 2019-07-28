using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal class AsyncWorkItem<T> : ITraceableWorkItem<T>
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<Task<T>> _Do;

        internal AsyncWorkItem(Func<Task<T>> @do)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
        }

        public async void Do()
        {
            try
            {
                _Source.TrySetResult(await _Do());
            }
            catch (Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }
}
