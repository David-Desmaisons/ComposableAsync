using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Queue
{


    internal class WorkItem<T> : IWorkItem
    {
        private TaskCompletionSource<T> _Source;
        private Func<T> _Do;

        public WorkItem(Func<T> iDo)
        {
            _Do = iDo;
            _Source = new TaskCompletionSource<T>();
        }


        public Task Task
        {
            get { return _Source.Task; }
        }

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
            catch(Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }

    internal class ActionWorkItem : WorkItem<object>, IWorkItem 
    {
        public ActionWorkItem(Action iDo)
            : base(() => { iDo(); return null; })
        {
        }
    }
}
