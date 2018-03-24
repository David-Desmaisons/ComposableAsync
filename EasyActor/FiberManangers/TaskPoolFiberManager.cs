using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    internal class TaskPoolFiberManager : IFiberMananger
    {
        public ActorFactorType Type => ActorFactorType.TaskPool;
        public bool DisposeFiber => true;

        public IFiber GetFiber() => Fiber.GetTaskBasedFiber();

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
