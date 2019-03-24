using Concurrent;
using Concurrent.Dispatchers;

namespace EasyActor
{
    /// <summary>
    /// <see cref="ICancellableDispatcherProvider"/> extension
    /// </summary>
    public static class FiberProviderExtension
    {
        /// <summary>
        /// Returns the underlying <see cref="IDispatcher"/>
        /// </summary>
        /// <param name="cancellableDispatcherProvider"></param>
        /// <returns></returns>
        public static IDispatcher GetAssociatedDispatcher(this ICancellableDispatcherProvider cancellableDispatcherProvider)
        {
            return cancellableDispatcherProvider?.Dispatcher ?? NullDispatcher.Instance;
        }
    }
}
