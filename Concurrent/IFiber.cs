﻿using System.Threading;

namespace Concurrent
{
    /// <summary>
    /// Fiber abstraction
    /// </summary>
    public interface IFiber : ICancellableDispatcher
    {
        /// <summary>
        /// True if the fiber is active
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Corresponding synchronization context
        /// </summary>
        SynchronizationContext SynchronizationContext { get; }
    }
}
