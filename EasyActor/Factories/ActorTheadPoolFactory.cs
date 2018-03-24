using Concurrent;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public class ActorTheadPoolFactory : ActorMonoTheadPoolFactory
    {
        public override ActorFactorType Type => ActorFactorType.ThreadPool;

        protected override IStopableFiber ObtainMonoFiber() => Fiber.GetThreadPoolFiber();
    }
}
