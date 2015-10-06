using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.TaskHelper
{
    public static class TaskBuilder
    {
        public static Task GetCompleted()
        {
            return Task.FromResult<object>(null);
        }

        public static Task<T> GetCancelled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }
    }
}
