using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManangers
{
    internal class SharedThreadFiberManager : IDispatcherMananger
    {
        public ActorFactorType Type => ActorFactorType.Shared;
        public bool DisposeDispatcher => false;
        private readonly IMonoThreadFiber _Fiber;

        public SharedThreadFiberManager(Action<Thread> onCreate = null)
        {
           _Fiber = Fiber.CreateMonoThreadedFiber(onCreate);
        }

        public IDispatcher GetDispatcher() => _Fiber;

        public Task DisposeAsync() => _Fiber.DisposeAsync();      
    }
}
