using Concurrent;
using EasyActor.Factories;

namespace EasyActor
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object rawimplementation)
        {
            var fiberProvider = rawimplementation as IFiberProvider;
            if (fiberProvider != null)
                return fiberProvider.Fiber;

            var description = ActorFactoryBase.GetCachedActor(rawimplementation);
            return description?.Fiber;
        }
    }
}
