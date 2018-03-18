using System.Threading;

namespace Concurrent.WPF.Internal
{
    public static class DispatcherExtension
    {
        internal static SynchronizationContext GetSynchronizationContext(this System.Windows.Threading.Dispatcher dispatcher)
        {
            SynchronizationContext result = null;
            dispatcher.Invoke(() => result = SynchronizationContext.Current);
            return result;
        }
    }
}
