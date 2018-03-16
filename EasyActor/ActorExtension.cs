using EasyActor.Factories;

namespace EasyActor
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object rawimplementation)
        {
            var description = ActorFactoryBase.GetCachedActor(rawimplementation);
            return description?.Fiber;
        }
    }
}
