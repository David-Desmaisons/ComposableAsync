using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    internal class SynchronizationContextFiberManage : IFiberMananger
    {
        public ActorFactorType Type => ActorFactorType.InCurrentContext;
        public bool DisposeFiber => false;
        private readonly IFiber _Fiber;

        public SynchronizationContextFiberManage() : this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFiberManage(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException(nameof(synchronizationContext), "synchronizationContext can not be null");
            _Fiber = Fiber.GetFiberFromSynchronizationContext(synchronizationContext);
        }

        public IFiber GetFiber() => _Fiber;

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
