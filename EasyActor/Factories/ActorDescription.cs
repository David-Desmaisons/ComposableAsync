using Concurrent;
using EasyActor.Options;

namespace EasyActor.Factories
{
    internal class ActorDescription
    {
        internal ActorDescription(object actor, IFiber fiber, ActorFactorType type)
        {
            ActorProxy = actor;
            Fiber = fiber;
            Type = type;
        }

        public object ActorProxy { get; }

        public IFiber Fiber { get; }

        public ActorFactorType Type { get; }
    }
}
