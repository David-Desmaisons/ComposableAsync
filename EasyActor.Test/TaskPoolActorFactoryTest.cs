using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using System.Threading;
using EasyActor.Test.TestInfra.DummyClass;

namespace EasyActor.Test
{
    [TestFixture]
    public class TaskPoolActorFactoryTest
    {
        private TaskPoolActorFactory _TaskPoolActorFactory;

        [SetUp]
        public void TestUp()
        {
            _TaskPoolActorFactory = new TaskPoolActorFactory();
        }

        [Test]
        public void Type_Should_Be_Standard()
        {
            _TaskPoolActorFactory.Type.Should().Be(ActorFactorType.TaskPool);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public async Task BuildAsync_Should_Call_Constructor_On_ThreadPoolThread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            IDummyInterface2 intface = await _TaskPoolActorFactory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.IsThreadPoolThread.Should().BeTrue();
        }

        [Test]
        public void Actor_Should_Implement_IActorLifeCycle()
        {
            var intface = _TaskPoolActorFactory.Build<IDummyInterface2>(new DummyClass());
            intface.Should().BeAssignableTo<IActorLifeCycle>();
        }

        [Test]
        public async Task Actor_IActorLifeCycle_Stop_Should_Call_Proxified_Class_On_IAsyncDisposable()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _TaskPoolActorFactory.Build<IDummyInterface1>(dispclass);

            //act
            var disp = intface as IActorLifeCycle;

            await disp.Stop();

            //assert
            dispclass.IsDisposed.Should().BeTrue();
        }

        [Test]
        public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IActorLifeCycle_Stop()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _TaskPoolActorFactory.Build<IDummyInterface1>(dispclass);

            //act
            var task = intface.DoAsync();

            var disp = intface as IActorLifeCycle;

            await disp.Stop();

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

        [Test]
        public async Task Actor_IActorLifeCycle_Stop_Should_Not_Cancel_Enqueued_Task()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _TaskPoolActorFactory.Build<IDummyInterface1>(dispclass);

            Task Taskrunning = intface.DoAsync(), Takenqueued = intface.DoAsync();
            Thread.Sleep(100);
            //act
            var disp = intface as IActorLifeCycle;

            await disp.Stop();
            await Takenqueued;

            //assert
            Takenqueued.IsCompleted.Should().BeTrue();
        }
    }
}
