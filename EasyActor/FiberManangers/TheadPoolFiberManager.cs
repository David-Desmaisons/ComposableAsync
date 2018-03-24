using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    internal class TheadPoolFiberManager : IFiberMananger
    {
        public ActorFactorType Type => ActorFactorType.ThreadPool;
        public bool DisposeFiber => true;

        public IFiber GetFiber() => Fiber.GetThreadPoolFiber();

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
