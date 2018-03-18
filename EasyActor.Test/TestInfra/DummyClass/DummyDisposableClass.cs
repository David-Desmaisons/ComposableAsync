using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class DisposableClass : IDummyInterface1, IAsyncDisposable
    {
        public DisposableClass()
        {
            IsDisposed = false;
        }

        public bool IsDisposed { get; private set; }

        public Task DoAsync()
        {
            Thread.Sleep(800);
            return Task.FromResult<object>(null);
        }

        public Task DoAsync(IProgress<int> progress)
        {
            progress.Report(1);
            Thread.Sleep(800);
            progress.Report(95);
            return Task.FromResult<object>(null);
        }

        public Task DisposeAsync()
        {
            Dispose();
            return TaskBuilder.Completed;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
