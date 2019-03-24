using System;
using Concurrent;

namespace EasyActor.DispatcherManagers
{
    /// <summary>
    /// Dispatcher manager
    /// </summary>
    public interface IDispatcherManager : IAsyncDisposable
    {
        /// <summary>
        /// true if the Dispatcher should be released
        /// </summary>
        bool DisposeDispatcher { get; }

        /// <summary>
        /// Returns a consumable Dispatcher
        /// </summary>
        /// <returns></returns>
        ICancellableDispatcher GetDispatcher();
    }
}
