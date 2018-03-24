using System.Threading.Tasks;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public sealed class TaskPoolActorFactory : ActorMonoTheadPoolFactory, IActorFactory
    {
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable();

        public override ActorFactorType Type => ActorFactorType.TaskPool;

        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();

        protected override IStopableFiber GetMonoFiber() => _ComposableAsyncDisposable.Add(Fiber.GetTaskBasedFiber());
    }
}
