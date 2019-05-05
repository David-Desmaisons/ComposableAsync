using System;

namespace Concurrent
{
    /// <summary>
    /// Cancellable Dispatcher that can be disposed
    /// </summary>
    public interface ICancellableDisposableDispatcher : ICancellableDispatcher, IAsyncDisposable
    {
    }
}
