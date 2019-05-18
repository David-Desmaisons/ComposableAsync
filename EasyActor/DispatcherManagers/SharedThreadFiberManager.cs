using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync;
using ComposableAsync.Concurrent;

namespace EasyActor.DispatcherManagers
{
    internal sealed class SharedThreadFiberManager : IDispatcherManager
    {
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
