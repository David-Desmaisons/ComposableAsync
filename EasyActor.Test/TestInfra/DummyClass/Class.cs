using EasyActor.TaskHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Test
{
    public interface Interface1
    {
        Task DoAsync();
    }

    public interface Interface : Interface1
    {
        Task SlowDoAsync();

        Task<int> ComputeAsync(int value);

        Task<Tuple<Thread, Thread>> DoAnRedoAsync();

        void Do();

        Task Throw();
    }
    public class Class : Interface
    {
        public Class()
        {
            CallingConstructorThread = Thread.CurrentThread;
        }
        public Thread CallingThread { get; private set; }

        public Thread CallingConstructorThread { get; private set; }

        public bool Done { get; set; }
        public Task DoAsync()
        {
            CallingThread = Thread.CurrentThread;
            Done = true;
            Thread.Sleep(200);
            return TaskBuilder.Completed;
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


        public Task Throw()
        {
            throw new Exception();
        }

        public Task SlowDoAsync()
        {
            CallingThread = Thread.CurrentThread;
            Thread.Sleep(1000);
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
    }

    public class DisposableClass : Interface1, IAsyncDisposable
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
