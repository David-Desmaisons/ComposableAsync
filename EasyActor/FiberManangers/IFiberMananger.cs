using System;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    public interface IFiberMananger : IAsyncDisposable
    {
        bool DisposeFiber { get; }

        ActorFactorType Type { get; }

        IFiber GetFiber();
    }
}
