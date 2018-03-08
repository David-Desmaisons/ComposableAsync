using EasyActor.Fiber;

namespace EasyActor.Factories
{
    public class ActorTheadPoolFactory : ActorMonoTheadPoolFactory
    {
        public override ActorFactorType Type => ActorFactorType.ThreadPool;

        protected override IMonoThreadFiber GetFiber() => new ThreadPoolFiber();       
    }
}
