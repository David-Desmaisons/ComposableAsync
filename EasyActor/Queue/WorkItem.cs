using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EasyActor.Queue
{
    internal class DispatchItem : IWorkItem
    {
        private readonly Action _Do;
        public DispatchItem(Action @do)
        {
            _Do = @do;
        }

        public void Cancel() {}

        public void Do() => _Do();
    }

    internal class WorkItem<T> : IWorkItem
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<T> _Do;

        public WorkItem(Func<T> iDo)
        {
            _Do = iDo;
            _Source = new TaskCompletionSource<T>();
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
        }

        [DebuggerNonUserCode]
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

    internal class ActionWorkItem : WorkItem<object>, IWorkItem
    {
        public ActionWorkItem(Action @do) : base(() => { @do(); return null; })
        {
        }
    }
}
