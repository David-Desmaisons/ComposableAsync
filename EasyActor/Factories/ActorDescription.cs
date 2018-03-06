using System.Threading.Tasks;

namespace EasyActor.Factories
{
    internal class ActorDescription
    {
        internal ActorDescription(object actor, TaskScheduler taskScheduler, ActorFactorType type)
        {
            ActorProxy = actor;
            TaskScheduler = taskScheduler;
            Type = type;
        }

        public object ActorProxy { get; }

        public TaskScheduler TaskScheduler { get; }

        public ActorFactorType Type { get; }
    }
}
