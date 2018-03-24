using System.Threading.Tasks;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public class ActorTheadPoolFactory : ActorMonoTheadPoolFactory, IActorFactory
    {
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();

        public override ActorFactorType Type => ActorFactorType.ThreadPool;

        protected override IStopableFiber GetMonoFiber() => _ComposableAsyncDisposable.Add(Fiber.GetThreadPoolFiber());

        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();
    }
}
