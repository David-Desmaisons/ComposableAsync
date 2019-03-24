using System;
using Concurrent;
using EasyActor.Options;

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
        /// Actor factory type
        /// </summary>
        ActorFactorType Type { get; }

        /// <summary>
        /// Returns a consumable Dispatcher
        /// </summary>
        /// <returns></returns>
        ICancellableDispatcher GetDispatcher();
    }
}
