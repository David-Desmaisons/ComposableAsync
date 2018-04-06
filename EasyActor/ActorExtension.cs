using Concurrent;
using Concurrent.Dispatchers;

namespace EasyActor
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object @object) => @object.GetAssociatedDispatcher() as IFiber;

        public static IDispatcher GetAssociatedDispatcher(this object @object)
        {
            var fiberProvider = @object as IFiberProvider;
            return fiberProvider?.Fiber ?? NullDispatcher.Instance;
        }
    }
}
