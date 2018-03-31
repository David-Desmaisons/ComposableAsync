using System;

namespace Concurrent
{
    public interface IStopableFiber : IFiber, IAsyncDisposable
    {
    }
}
