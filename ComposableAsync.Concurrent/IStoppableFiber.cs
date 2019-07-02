using System;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// Fiber that can be stopped
    /// </summary>
    public interface IStoppableFiber : IFiber, IAsyncDisposable
    {
    }
}
