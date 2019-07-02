using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.DispatcherManagers
{
    internal sealed class TaskPoolFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => true;

        public IDispatcher GetDispatcher() => Fiber.GetTaskBasedFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
