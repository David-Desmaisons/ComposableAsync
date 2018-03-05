using System;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyActor.TaskHelper
{
    public static class TaskBuilder
    {
        private static readonly Task _Completed;

        static TaskBuilder()
        {
            _Completed = Task.FromResult<object>(null);

            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            Cancelled = tcs.Task;
        }

        public static Task Completed => _Completed;

        public static Task Cancelled { get; }

        private static Task<T> PrivateGetCancelled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        private static readonly MethodInfo _GetCancelled = typeof(TaskBuilder).GetMethod("PrivateGetCancelled", BindingFlags.Static | BindingFlags.NonPublic);

        internal static Task GetCancelled(this Type @this)
        {
            var task = @this.GetTaskType();
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
        static TaskBuilder()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            Cancelled = tcs.Task;
        }

        public static Task<T> Cancelled { get; }
    }
}
