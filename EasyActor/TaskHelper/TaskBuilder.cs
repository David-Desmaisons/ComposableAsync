using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static Task<T> PrivateGetCancelled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        private static MethodInfo _GetCancelled = typeof(TaskBuilder).GetMethod("PrivateGetCancelled", BindingFlags.Static | BindingFlags.NonPublic);

        internal static Task GetCancelled(this Type @this)
        {
            TaskDescription task = @this.GetTaskType();
            switch (task.MethodType)
            {                
                case TaskType.Task:
                    return Cancelled;

                case TaskType.GenericTask:
                    return (Task)_GetCancelled.MakeGenericMethod(task.Type).Invoke(null, new object[] { });    
            }  
            
            return null;
        }
    }

    public static class TaskBuilder<T>
    {
        private static Task<T> _Cancelled;
        static TaskBuilder()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            _Cancelled = tcs.Task;
        }

        public static Task<T> Cancelled
        {
            get {return _Cancelled;}
        }
    }
}
