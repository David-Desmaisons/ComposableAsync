using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using EasyActor.Factories;
using EasyActor.Options;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;
using Concurrent;

namespace EasyActor.Test
{     
    public class TaskPoolActorFactoryTest
    {
        private readonly TaskPoolActorFactory _TaskPoolActorFactory;

        public TaskPoolActorFactoryTest()
        {
            _TaskPoolActorFactory = new TaskPoolActorFactory();
        }

        [Fact]
        public void Type_Should_Be_Standard()
        {
            _TaskPoolActorFactory.Type.Should().Be(ActorFactorType.TaskPool);
        }

        [Fact]
        public async Task Method_Should_Run_On_Separated_Pooled_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var intface = _TaskPoolActorFactory.Build<IDummyInterface2>(target);
            await intface.DoAsync();

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
            var intface = _TaskPoolActorFactory.Build<IDummyInterface2>(target);
            var res = await intface.ComputeAsync(2);

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
            var intface = _TaskPoolActorFactory.Build<IDummyInterface2>(target);
            await intface.DoAsync();

            var thread = target.CallingThread;

            await intface.DoAsync();
            target.CallingThread.IsThreadPoolThread.Should().BeTrue();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Each_Actor_Should_Run_On_Separated_Thread()
        {
            //arrange
            var target1 = new DummyClass();
            var target2 = new DummyClass();
            var intface1 = _TaskPoolActorFactory.Build<IDummyInterface2>(target1);
            var intface2 = _TaskPoolActorFactory.Build<IDummyInterface2>(target2);

            //act
            await Task.WhenAll(intface1.SlowDoAsync(), intface2.SlowDoAsync());

            //assert
            target1.CallingThread.Should().NotBe(target2.CallingThread);
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_ThreadPoolThread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            var intface = await _TaskPoolActorFactory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IAsyncDisposable_DisposeAsync()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _TaskPoolActorFactory.Build<IDummyInterface1>(dispclass);

            //act
            var task = intface.DoAsync();

            var disp = GetFiber(intface) as IAsyncDisposable;

            await disp.DisposeAsync();

            TaskCanceledException error = null;
            Task canc = null;
            try
            {
                canc = intface.DoAsync();
                await canc;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            //assert
            error.Should().NotBeNull();
            task.IsCompleted.Should().BeTrue();
            canc.IsCanceled.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_IAsyncDisposable_DisposeAsync_Should_Not_Cancel_Enqueued_Task()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _TaskPoolActorFactory.Build<IDummyInterface1>(dispclass);

            Task taskrunning = intface.DoAsync();
            var takenqueued = intface.DoAsync();
            Thread.Sleep(100);
            //act
            var disp = GetFiber(intface) as IAsyncDisposable;

            await disp.DisposeAsync();
            await takenqueued;

            //assert
            takenqueued.IsCompleted.Should().BeTrue();
        }

        private IFiber GetFiber<T>(T actor) => (actor as IFiberProvider)?.Fiber;
    }
}
