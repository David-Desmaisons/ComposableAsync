using System.Threading;

namespace ComposableAsync.WPF
{
    internal static class DispatcherExtension
    {
        internal static SynchronizationContext GetSynchronizationContext(this System.Windows.Threading.Dispatcher dispatcher)
        {
            return dispatcher.Invoke(() => SynchronizationContext.Current);
        }
    }
}
