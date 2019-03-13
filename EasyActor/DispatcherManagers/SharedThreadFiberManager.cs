using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManagers
{
    internal class SharedThreadFiberManager : IDispatcherManager
    {
        public ActorFactorType Type => ActorFactorType.Shared;
        public bool DisposeDispatcher => false;
        private readonly IMonoThreadFiber _Fiber;

        public SharedThreadFiberManager(Action<Thread> onCreate = null)
        {
           _Fiber = Fiber.CreateMonoThreadedFiber(onCreate);
        }

        public ICancellableDispatcher GetDispatcher() => _Fiber;

        public Task DisposeAsync() => _Fiber.DisposeAsync();      
    }
}
