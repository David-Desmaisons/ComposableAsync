using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Queue
{
   

    public sealed class DispatcherSynchronizationContext : SynchronizationContext
    {
        private readonly AsyncQueueMonoThreadDispatcher _dispatcher;


        public DispatcherSynchronizationContext(AsyncQueueMonoThreadDispatcher dispatcher)
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

        public bool IsSame(DispatcherSynchronizationContext iother)
        {
            return _dispatcher == iother._dispatcher;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new DispatcherSynchronizationContext(_dispatcher);
        }
    }
}
