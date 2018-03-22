using System;

namespace Concurrent.Disposable
{
    public interface IShareableAsyncDisposable
    {
        IAsyncDisposable GetDisposable();
    }
}
