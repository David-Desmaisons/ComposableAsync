using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManagers
{
    internal class StandardFiberManager : IDispatcherManager
    {
        public ActorFactorType Type => ActorFactorType.Standard;
        public bool DisposeDispatcher => true;

        private readonly Action<Thread> _OnCreate;
        public StandardFiberManager(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public ICancellableDispatcher GetDispatcher() => Fiber.CreateMonoThreadedFiber(_OnCreate);

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
