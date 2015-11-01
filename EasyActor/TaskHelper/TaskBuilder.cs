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

        public static Task<T> GetCancelled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        private static MethodInfo _GetCancelled = typeof(TaskBuilder).GetMethod("GetCancelled", BindingFlags.Static | BindingFlags.Public);


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
}
