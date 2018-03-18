﻿using System;
using System.Threading;

namespace Concurrent.SynchronizationContexts
{
    internal sealed class MonoThreadedFiberSynchronizationContext : SynchronizationContext
    {
        private readonly IMonoThreadFiber _Dispatcher;

        public MonoThreadedFiberSynchronizationContext(IMonoThreadFiber dispatcher)
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
