﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Concurrent.WorkItems
{
    public class WorkItem<T> : IWorkItem
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<T> _Do;
        private bool _Cancelled = false;

        public WorkItem(Func<T> @do)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
            _Cancelled = true;
        }

        [DebuggerNonUserCode]
        public void Do()
        {
            if (_Cancelled)
                return;

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
