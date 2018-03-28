using Concurrent;
using EasyActor.Options;

namespace EasyActor.Factories
{
    internal class ActorDescription
    {
        internal ActorDescription(object actor, IDispatcher dispatcher, ActorFactorType type)
        {
            ActorProxy = actor;
            Dispatcher = dispatcher;
            Type = type;
        }

        public object ActorProxy { get; }

        public IDispatcher Dispatcher { get; }

        public ActorFactorType Type { get; }
    }
}
