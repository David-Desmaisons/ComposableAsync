using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent;

namespace ComposableAsync.Actors.DispatcherManagers
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
