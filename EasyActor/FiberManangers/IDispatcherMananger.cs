using System;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    public interface IDispatcherMananger : IAsyncDisposable
    {
        bool DisposeDispatcher { get; }

        ActorFactorType Type { get; }

        IDispatcher GetDispatcher();
    }
}
