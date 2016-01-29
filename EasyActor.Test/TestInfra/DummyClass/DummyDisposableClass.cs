using EasyActor.TaskHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
