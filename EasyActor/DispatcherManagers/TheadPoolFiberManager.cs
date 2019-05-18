using System.Threading.Tasks;
using ComposableAsync;
using Concurrent;

namespace EasyActor.DispatcherManagers
{
    internal sealed class TheadPoolFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => true;

        public ICancellableDispatcher GetDispatcher() => Fiber.GetThreadPoolFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
