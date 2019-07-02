using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal class WorkItem<T> : ITraceableWorkItem<T>
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<T> _Do;

        internal WorkItem(Func<T> @do)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
        }

        public void Do()
        {
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
