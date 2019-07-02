using System.Threading.Tasks;
using ComposableAsync.Concurrent.TaskSchedulers;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// <see cref="IFiber"/> extension
    /// </summary>
    public static class FiberExtension
    {
        /// <summary>
        /// Returns a task scheduler corresponding to the given fiber
        /// </summary>
        /// <param name="fiber"></param>
        /// <returns></returns>
        public static TaskScheduler GetTaskScheduler(this IFiber fiber)
        {
            return new FiberTaskScheduler(fiber);
        }
    }
}
