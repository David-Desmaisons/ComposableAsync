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

        public object ActorProxy { get; private set; }

        public TaskScheduler TaskScheduler { get; private set; }

        public ActorFactorType Type { get; private set; }
    }
}
