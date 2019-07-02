using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.DispatcherManagers
{
    internal sealed class SharedThreadFiberManager : IDispatcherManager
    {
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
