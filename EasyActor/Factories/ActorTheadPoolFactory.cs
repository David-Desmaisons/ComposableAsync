using EasyActor.Fiber;

namespace EasyActor.Factories
{
    public class ActorTheadPoolFactory : ActorMonoTheadPoolFactory
    {
        public override ActorFactorType Type => ActorFactorType.ThreadPool;

        protected override IAbortableFiber GetMonoFiber() => new ThreadPoolFiber();
    }
}
