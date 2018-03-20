using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class DisposableClass : IDummyInterface4
    {
        public DisposableClass()
        {
            IsDisposed = false;
        }

        public Thread LastCallingThread { get; private set; }

        public bool IsDisposed { get; private set; }

        public Task DoAsync()
        {
            LastCallingThread = Thread.CurrentThread;
            Thread.Sleep(800);
            return Task.FromResult<object>(null);
        }

        public Task DoAsync(IProgress<int> progress)
        {
            LastCallingThread = Thread.CurrentThread;
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
            LastCallingThread = Thread.CurrentThread;
            IsDisposed = true;
        }
    }
}
