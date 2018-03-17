using Concurrent;
using Concurrent.Fibers;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public class ActorTheadPoolFactory : ActorMonoTheadPoolFactory
    {
        public override ActorFactorType Type => ActorFactorType.ThreadPool;

        protected override IAbortableFiber GetMonoFiber() => Fiber.GetThreadPoolFiber();
    }
}
