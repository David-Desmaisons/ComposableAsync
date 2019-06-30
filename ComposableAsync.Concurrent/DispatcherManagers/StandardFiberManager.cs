using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.DispatcherManagers
{
    internal sealed class StandardFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => true;

        private readonly Action<Thread> _OnCreate;
        public StandardFiberManager(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public IDispatcher GetDispatcher() => Fiber.CreateMonoThreadedFiber(_OnCreate);

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
