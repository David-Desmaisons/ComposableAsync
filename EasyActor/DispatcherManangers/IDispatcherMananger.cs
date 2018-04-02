using System;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManangers
{
    public interface IDispatcherMananger : IAsyncDisposable
    {
        bool DisposeDispatcher { get; }

        ActorFactorType Type { get; }

        ICancellableDispatcher GetDispatcher();
    }
}
