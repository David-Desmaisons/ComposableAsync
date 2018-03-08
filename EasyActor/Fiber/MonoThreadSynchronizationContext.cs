using System;
using System.Threading;

namespace EasyActor.Fiber
{
    public sealed class MonoThreadedQueueSynchronizationContext : SynchronizationContext
    {
        private readonly IMonoThreadFiber _Dispatcher;

        public MonoThreadedQueueSynchronizationContext(IMonoThreadFiber dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));

            _Dispatcher = dispatcher;

            SetWaitNotificationRequired();
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _Dispatcher.Send(() => d(state));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _Dispatcher.Enqueue(() => d(state));
        }

        public bool IsSame(MonoThreadedQueueSynchronizationContext iother)
        {
            return _Dispatcher == iother._Dispatcher;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new MonoThreadedQueueSynchronizationContext(_Dispatcher);
        }
    }
}
