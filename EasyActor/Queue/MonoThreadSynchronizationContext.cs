using System;
using System.Threading;

namespace EasyActor.Queue
{
    public sealed class MonoThreadedQueueSynchronizationContext : SynchronizationContext
    {
        private readonly MonoThreadedQueue _dispatcher;

        public MonoThreadedQueueSynchronizationContext(MonoThreadedQueue dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            _dispatcher = dispatcher;

            SetWaitNotificationRequired();
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _dispatcher.Send(() => d(state));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _dispatcher.Enqueue(() => d(state));
        }

        public bool IsSame(MonoThreadedQueueSynchronizationContext iother)
        {
            return _dispatcher == iother._dispatcher;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new MonoThreadedQueueSynchronizationContext(_dispatcher);
        }
    }
}
