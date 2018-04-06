using Concurrent;
using Concurrent.Dispatchers;

namespace EasyActor
{
    public static class FiberProviderExtension
    {
        public static IDispatcher GetAssociatedDispatcher(this IFiberProvider fiberProvider)
        {
            return fiberProvider?.Fiber ?? NullDispatcher.Instance;
        }
    }
}
