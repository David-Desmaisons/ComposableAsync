using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    internal class SharedThreadFiberManager : IFiberMananger
    {
        public ActorFactorType Type => ActorFactorType.Shared;
        public bool DisposeFiber => false;
        private readonly IMonoThreadFiber _Fiber;

        public SharedThreadFiberManager(Action<Thread> onCreate = null)
        {
           _Fiber = Fiber.CreateMonoThreadedFiber(onCreate);
        }

        public IFiber GetFiber() => _Fiber;

        public Task DisposeAsync() => _Fiber.DisposeAsync();      
    }
}
