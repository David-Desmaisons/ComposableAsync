using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Factory;
using ComposableAsync.Factory.Test.TestInfra.DummyClass;
using FluentAssertions;
using Xunit;

namespace ComposableAsync.Concurrent.Test
{     
    public class TaskPoolActorFactoryTest
    {
        private readonly IProxyFactory _TaskPoolActorFactory;

        public TaskPoolActorFactoryTest()
        {
            _TaskPoolActorFactory = new ActorFactoryBuilder().GetTaskBasedActorFactory();
        }

        [Fact]
        public async Task Method_Should_Run_On_Separated_Pooled_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var @interface = _TaskPoolActorFactory.Build<IDummyInterface2>(target);
            await @interface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.IsThreadPoolThread.Should().BeTrue();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Method_WithAReturn_Should_Run_On_Separated_Pooled_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var @interface = _TaskPoolActorFactory.Build<IDummyInterface2>(target);
            var res = await @interface.ComputeAsync(2);

            res.Should().Be(2);
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.IsThreadPoolThread.Should().BeTrue();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Method_Should_Not_Always_OnPoolThread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var @interface = _TaskPoolActorFactory.Build<IDummyInterface2>(target);
            await @interface.DoAsync();

            var thread = target.CallingThread;

            await @interface.DoAsync();
            target.CallingThread.IsThreadPoolThread.Should().BeTrue();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Each_Actor_Should_Run_On_Separated_Thread()
        {
            //arrange
            var target1 = new DummyClass();
            var target2 = new DummyClass();
            var @interface1 = _TaskPoolActorFactory.Build<IDummyInterface2>(target1);
            var @interface2 = _TaskPoolActorFactory.Build<IDummyInterface2>(target2);

            //act
            await Task.WhenAll(@interface1.SlowDoAsync(), @interface2.SlowDoAsync());

            //assert
            target1.CallingThread.Should().NotBe(target2.CallingThread);
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_ThreadPoolThread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            var @interface = await _TaskPoolActorFactory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await @interface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IAsyncDisposable_DisposeAsync()
        {
            //arrange
            var disposableClass = new DisposableClass();
            var @interface = _TaskPoolActorFactory.Build<IDummyInterface1>(disposableClass);

            //act
            var task = @interface.DoAsync();

            var disposable = GetFiber(@interface) as IAsyncDisposable;

            await disposable.DisposeAsync();

            TaskCanceledException error = null;
            Task cancellable = null;
            try
            {
                cancellable = @interface.DoAsync();
                await cancellable;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            //assert
            error.Should().NotBeNull();
            task.IsCompleted.Should().BeTrue();
            cancellable.IsCanceled.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_IAsyncDisposable_DisposeAsync_Should_Not_Cancel_Enqueued_Task()
        {
            //arrange
            var disposable = new DisposableClass();
            var @interface = _TaskPoolActorFactory.Build<IDummyInterface1>(disposable);

            Task taskRunning = @interface.DoAsync();
            var taskEnqueued = @interface.DoAsync();
            Thread.Sleep(100);
            //act
            var disposable2 = GetFiber(@interface) as IAsyncDisposable;

            await disposable2.DisposeAsync();
            await taskEnqueued;

            //assert
            taskEnqueued.IsCompleted.Should().BeTrue();
        }

        private ICancellableDispatcher GetFiber<T>(T actor) => (actor as ICancellableDispatcherProvider)?.Dispatcher;
    }
}
