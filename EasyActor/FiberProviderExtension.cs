using Concurrent;
using Concurrent.Dispatchers;

namespace EasyActor
{
    /// <summary>
    /// <see cref="IFiberProvider"/> extension
    /// </summary>
    public static class FiberProviderExtension
    {
        /// <summary>
        /// Returns the underlying <see cref="IDispatcher"/>
        /// </summary>
        /// <param name="fiberProvider"></param>
        /// <returns></returns>
        public static IDispatcher GetAssociatedDispatcher(this IFiberProvider fiberProvider)
        {
            return fiberProvider?.Fiber ?? NullDispatcher.Instance;
        }
    }
}
