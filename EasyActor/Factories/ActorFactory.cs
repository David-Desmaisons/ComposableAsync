using System;
using System.Threading;
using EasyActor.Fiber;

namespace EasyActor.Factories
{
    public sealed class ActorFactory : ActorMonoTheadPoolFactory
    {
        private readonly Action<Thread> _OnCreate;

        public ActorFactory(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public override ActorFactorType Type => ActorFactorType.Standard;

        protected override IMonoThreadFiber GetMonoFiber() => new MonoThreadedFiber(_OnCreate);
    }
}
