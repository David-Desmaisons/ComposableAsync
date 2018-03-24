using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Disposable;
using EasyActor.Options;

namespace EasyActor.Factories
{
    public sealed class ActorFactory : ActorMonoTheadPoolFactory, IActorFactory
    {
        private readonly Action<Thread> _OnCreate;
        private readonly ComposableAsyncDisposable _ComposableAsyncDisposable = new ComposableAsyncDisposable(); 

        public ActorFactory(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public override ActorFactorType Type => ActorFactorType.Standard;

        protected override IStopableFiber GetMonoFiber() => _ComposableAsyncDisposable.Add(Fiber.CreateMonoThreadedFiber(_OnCreate));

        public Task DisposeAsync() => _ComposableAsyncDisposable.DisposeAsync();
    }
}
