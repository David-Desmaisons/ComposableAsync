using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManangers
{
    internal class TaskPoolFiberManager : IDispatcherMananger
    {
        public ActorFactorType Type => ActorFactorType.TaskPool;
        public bool DisposeDispatcher => true;

        public ICancellableDispatcher GetDispatcher() => Fiber.GetTaskBasedFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
