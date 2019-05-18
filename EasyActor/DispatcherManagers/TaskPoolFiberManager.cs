using System.Threading.Tasks;
using ComposableAsync;
using Concurrent;

namespace EasyActor.DispatcherManagers
{
    internal sealed class TaskPoolFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => true;

        public ICancellableDispatcher GetDispatcher() => Fiber.GetTaskBasedFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
