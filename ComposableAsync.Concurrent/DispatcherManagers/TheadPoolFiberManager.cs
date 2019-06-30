using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.DispatcherManagers
{
    internal sealed class TheadPoolFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => true;

        public IDispatcher GetDispatcher() => Fiber.GetThreadPoolFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
