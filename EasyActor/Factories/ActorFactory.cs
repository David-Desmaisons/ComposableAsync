using System;
using System.Threading;
using Concurrent;
using EasyActor.Options;

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

        protected override IStopableFiber GetMonoFiber() => Fiber.CreateMonoThreadedFiber(_OnCreate);
    }
}
