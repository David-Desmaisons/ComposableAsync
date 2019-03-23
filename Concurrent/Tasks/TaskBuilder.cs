using System.Threading.Tasks;

namespace Concurrent.Tasks 
{
    /// <summary>
    /// Task utility
    /// </summary>
    public static class TaskBuilder
    {
        static TaskBuilder()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            Cancelled = tcs.Task;
        }

        /// <summary>
        /// Returns a cancelled task
        /// </summary>
        public static Task Cancelled { get; }
    }

    /// <summary>
    /// Task utility
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TaskBuilder<T>
    {
        static TaskBuilder()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            Cancelled = tcs.Task;
        }

        /// <summary>
        /// Returns a cancelled Task
        /// </summary>
        public static Task<T> Cancelled { get; }
    }
}
