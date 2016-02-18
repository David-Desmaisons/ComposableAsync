using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Queue
{
    internal class AsyncWorkItem<T> : IWorkItem
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<Task<T>> _Do;

        public AsyncWorkItem(Func<Task<T>> iDo)
        {
            _Do = iDo;
            _Source = new TaskCompletionSource<T>();
        }

        public Task<T> Task
        {
            get { return _Source.Task; }
        }

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
            catch(Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }

    internal class AsyncActionWorkItem : AsyncWorkItem<object>, IWorkItem 
    {
        public AsyncActionWorkItem(Func<Task> iDo)
            : base(async () => { await iDo(); return null; })
        {
        }
    }
}
