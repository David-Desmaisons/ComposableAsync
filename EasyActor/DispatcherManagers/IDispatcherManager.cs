using System;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManagers
{
    public interface IDispatcherManager : IAsyncDisposable
    {
        bool DisposeDispatcher { get; }

        ActorFactorType Type { get; }

        ICancellableDispatcher GetDispatcher();
    }
}
