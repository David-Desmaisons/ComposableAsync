using Concurrent;
using Concurrent.Dispatchers;
using EasyActor.Factories;

namespace EasyActor
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object @object)
                        => @object.GetAssociatedDispatcher() as IFiber;

        public static IDispatcher GetAssociatedDispatcher(this object @object)
        {
            var fiberProvider = @object as IFiberProvider;
            if (fiberProvider != null)
                return fiberProvider.Fiber;

            var description = ActorFactory.GetCachedActor(@object);
            return description?.Dispatcher ?? NullDispatcher.Instance;
        }
    }
}
