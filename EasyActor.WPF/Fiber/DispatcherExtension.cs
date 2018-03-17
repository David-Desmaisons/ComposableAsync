using System.Threading.Tasks;

namespace Concurrent.WPF.Fiber
{
    public static class DispatcherExtension
    {
        internal static TaskScheduler GetScheduler(this System.Windows.Threading.Dispatcher dispatcher)
        {
            TaskScheduler result = null;
            dispatcher.Invoke(() => result = TaskScheduler.FromCurrentSynchronizationContext());
            return result;
        }
    }
}
