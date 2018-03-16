using System.Threading.Tasks;
using System.Windows.Threading;

namespace EasyActor.WPF.Fiber
{
    public static class DispatcherExtension
    {
        internal static TaskScheduler GetScheduler(this Dispatcher dispatcher)
        {
            TaskScheduler result = null;
            dispatcher.Invoke(() => result = TaskScheduler.FromCurrentSynchronizationContext());
            return result;
        }
    }
}
