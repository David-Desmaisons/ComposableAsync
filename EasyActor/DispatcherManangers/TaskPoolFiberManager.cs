using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.DispatcherManangers
{
    internal class TaskPoolFiberManager : IDispatcherMananger
    {
        public ActorFactorType Type => ActorFactorType.TaskPool;
        public bool DisposeDispatcher => true;

        public IDispatcher GetDispatcher() => Fiber.GetTaskBasedFiber();

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
