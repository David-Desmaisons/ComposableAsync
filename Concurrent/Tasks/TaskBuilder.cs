using System.Threading.Tasks;

namespace Concurrent.Tasks 
{
    public static class TaskBuilder
    {
        static TaskBuilder()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            Cancelled = tcs.Task;
        }

        public static Task Cancelled { get; }

        private static Task<T> PrivateGetCancelled<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
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
