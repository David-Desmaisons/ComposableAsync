using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.DispatcherManangers
{
    internal class TheadPoolFiberManager : IDispatcherMananger
    {
        public ActorFactorType Type => ActorFactorType.ThreadPool;
        public bool DisposeDispatcher => true;

        public IDispatcher GetDispatcher() => Fiber.GetThreadPoolFiber();

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
