using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManagers
{
    internal class TheadPoolFiberManager : IDispatcherManager
    {
        public ActorFactorType Type => ActorFactorType.ThreadPool;
        public bool DisposeDispatcher => true;

        public ICancellableDispatcher GetDispatcher() => Fiber.GetThreadPoolFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
