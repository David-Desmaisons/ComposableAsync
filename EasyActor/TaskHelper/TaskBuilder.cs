using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.TaskHelper
{
    public static class TaskBuilder
    {
        private static Task _Completed;
        private static Task _Cancelled;
        static TaskBuilder()
        {
            _Completed = Task.FromResult<object>(null);

            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            _Cancelled = tcs.Task;
        }

        public static Task Completed
        {
            get { return _Completed; }
        }

        public static Task Cancelled
        {
            get {return _Cancelled;}
        }

        public static Task<T> GetCancelled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }
    }
}
