using EasyActor.TaskHelper;
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

        public Thread CallingConstructorThread { get; private set; }

        public bool Done { get; set; }
        public Task DoAsync()
        {
            DoAsyncCount++;
            CallingThread = Thread.CurrentThread;
            Done = true;
            Thread.Sleep(200);
            return TaskBuilder.Completed;
        }

        public Task DoAsync(IProgress<int> Progress) {
            DoAsyncCount++;
            CallingThread = Thread.CurrentThread;
            Done = true;
            Progress.Report(10);
            Thread.Sleep(200);
            Progress.Report(95);
            return TaskBuilder.Completed;
        }

        public async Task<Tuple<Thread, Thread>> DoAnRedoAsync()
        {
            CallingThread = Thread.CurrentThread;
            var one = CallingThread;

            await Task.Run(() => { Thread.Sleep(1000); });

            Done = true;
            return new Tuple<Thread, Thread>(one, CallingThread);
        }

        public void Do()
        {
        }

        public Task Throw()
        {
            throw new Exception();
        }

        public Task SlowDoAsync()
        {
            SlowDoAsyncCount++;
            CallingThread = Thread.CurrentThread;
            Thread.Sleep(1000);
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Done = true;
            return TaskBuilder.Completed;
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
            Thread.Sleep( TimeSpan.FromMilliseconds((double)(value * 1000)));
            Done = true;
            return Task.FromResult<decimal>(value);
        }
    }
}
