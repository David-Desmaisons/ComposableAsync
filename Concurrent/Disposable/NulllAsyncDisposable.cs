using System;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Disposable
{
    public sealed class NullAsyncDisposable : IAsyncDisposable
    {
        private NullAsyncDisposable() { }

        public void Dispose() { }

        public Task DisposeAsync() => TaskBuilder.Completed;

        public static IAsyncDisposable Null { get; } = new NullAsyncDisposable();
    }
}
