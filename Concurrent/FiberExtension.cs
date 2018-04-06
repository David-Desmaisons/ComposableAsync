using System.Threading.Tasks;
using Concurrent.TaskSchedulers;

namespace Concurrent
{
    public static class FiberExtension
    {
        public static TaskScheduler GetTaskScheduler(this IFiber fiber)
        {
            return new FiberTaskScheduler(fiber);
        }
    }
}
