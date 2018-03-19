using Concurrent;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public sealed class TaskPoolActorFactory : ActorMonoTheadPoolFactory
    {
        public override ActorFactorType Type => ActorFactorType.TaskPool;

        protected override IStopableFiber GetMonoFiber() => Fiber.GetTaskBasedFiber();
    }
}
