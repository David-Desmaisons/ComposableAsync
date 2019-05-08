using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class DummyClass : IDummyInterface2
    {
        public DummyClass()
        {
            CallingConstructorThread = Thread.CurrentThread;
        }

        public int DoAsyncCount { get; private set; }

        public int SlowDoAsyncCount { get; private set; }

        public Thread CallingThread { get; private set; }

        public Thread CallingConstructorThread { get; }

        public bool Done { get; set; }
        public Task DoAsync()
        {
            DoAsyncCount++;
            CallingThread = Thread.CurrentThread;
            Done = true;
            Thread.Sleep(200);
            return Task.CompletedTask;
        }

        public Task DoAsync(IProgress<int> progress)
        {
            DoAsyncCount++;
            CallingThread = Thread.CurrentThread;
            Done = true;
            progress.Report(10);
            Thread.Sleep(200);
            progress.Report(95);
            return Task.CompletedTask;
        }

        public async Task<Tuple<Thread, Thread>> DoAnRedoAsync()
        {
            CallingThread = Thread.CurrentThread;
            var one = CallingThread;

            await Task.Run(() => { Thread.Sleep(1000); });

            Done = true;
            return new Tuple<Thread, Thread>(one, Thread.CurrentThread);
        }

        public void Do()
        {
        }

        public Task ThrowAsync(Exception exception = null)
        {
            exception = exception ?? new Exception();
            throw exception;
        }

        public Task<int> ThrowAsyncWithResult(Exception exception = null)
        {
            exception = exception ?? new Exception();
            throw exception;
        }

        public void Throw(Exception exception = null)
        {
            exception = exception ?? new Exception();
            throw exception;
        }

        public Task SlowDoAsync()
        {
            SlowDoAsyncCount++;
            CallingThread = Thread.CurrentThread;
            Thread.Sleep(1000);
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Done = true;
            return Task.CompletedTask;
        }

        public Task<int> ComputeAsync(int value)
        {
            CallingThread = Thread.CurrentThread;
            Thread.Sleep(1000);
            Done = true;
            return Task.FromResult<int>(value);
        }

        public Task<decimal> ComputeAndWaitAsync(decimal value)
        {
            CallingThread = Thread.CurrentThread;
            Thread.Sleep(TimeSpan.FromMilliseconds((double)(value * 1000)));
            Done = true;
            return Task.FromResult<decimal>(value);
        }
    }
}
