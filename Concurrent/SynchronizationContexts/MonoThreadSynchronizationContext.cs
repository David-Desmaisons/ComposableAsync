using System;
using System.Diagnostics;
using System.Threading;

namespace Concurrent.SynchronizationContexts
{
    [DebuggerNonUserCode]
    internal sealed class MonoThreadedFiberSynchronizationContext : SynchronizationContext
    {
        private readonly IMonoThreadFiber _Dispatcher;

        public MonoThreadedFiberSynchronizationContext(IMonoThreadFiber dispatcher)
        {
            _Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
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

        public bool IsSame(MonoThreadedFiberSynchronizationContext other)
        {
            return _Dispatcher == other._Dispatcher;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new MonoThreadedFiberSynchronizationContext(_Dispatcher);
        }
    }
}
