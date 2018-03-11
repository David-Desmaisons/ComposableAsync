using System;
using System.Threading.Tasks;
using System.Threading;
using EasyActor.Factories;
using FluentAssertions;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;

namespace EasyActor.Test
{  
    public class ActorFactoryTest
    {
        private readonly ActorFactory _Factory;

        public ActorFactoryTest()
        {
            _Factory = new ActorFactory();
        }

        [Fact]
        public void Type_Should_Be_Standard()
        {
            _Factory.Type.Should().Be(ActorFactorType.Standard);
        }

        [Fact]
        public async Task Method_Should_Run_On_Separated_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);

            await (intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Method_Should_Always_Run_On_Same_Thread()
        {
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);
            await intface.DoAsync();

            var thread = target.CallingThread;

            await intface.DoAsync();

            target.CallingThread.Should().Be(thread);

            await (intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Each_Actor_Should_Run_On_Separated_Thread_When_Shared_Thread_Is_False()
        {
            //arrange
            var target1 = new DummyClass();
            var target2 = new DummyClass();
            var intface1 = _Factory.Build<IDummyInterface2>(target1);
            var intface2 = _Factory.Build<IDummyInterface2>(target2);

            //act
            await intface1.DoAsync();
            await intface2.DoAsync();

            //assert
            target1.CallingThread.Should().NotBe(target2.CallingThread);

            await (intface1 as IActorLifeCycle).Stop();
            await (intface2 as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Method_Should_Run_On_Same_Thread_After_Await()
        {
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);

            var res = await intface.DoAnRedoAsync();

            res.Item1.Should().Be(res.Item2);

            await (intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Build_Should_CreateSameInterface_ForSamePOCO()
        {
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);
            var intface2 = _Factory.Build<IDummyInterface2>(target);

            intface.Should().BeSameAs(intface2);

            await(intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Build_Should_Throw_Exception_IsSamePOCO_HasBeenUsedWithOtherFactory()
        {
            var target = new DummyClass();
            var sharedFactory = new SharedThreadActorFactory();
            var intface = sharedFactory.Build<IDummyInterface2>(target);

            Action Do = () => _Factory.Build<IDummyInterface2>(target);

            Do.ShouldThrow<ArgumentException>().And.Message.Should().Contain("Shared");

            await sharedFactory.DisposeAsync();
        }

        [Fact]
        public async Task Task_returned_By_Method_Should_Be_Awaited()
        {
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);
            await intface.SlowDoAsync();

            target.Done.Should().BeTrue();

            await (intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Method_With_Task_T_Should_Run_On_Separated_Tread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);
            var result = await intface.ComputeAsync(25);

            result.Should().Be(25);
            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);

            await (intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Method_returning_void_Task_Should_Not_Throw_Exception()
        {
            var intface = _Factory.Build<IDummyInterface2>(new DummyClass());
            Action Do = () => intface.Do();
            Do.ShouldNotThrow();

            await(intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            IDummyInterface2 intface = await _Factory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.Should().Be(target.CallingThread);

            await (intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Actor_Should_Implement_IActorLifeCycle_Even_If_Wrapped_Not_IDisposableAsync()
        {
            var intface = _Factory.Build<IDummyInterface2>(new DummyClass());
            intface.Should().BeAssignableTo<IActorCompleteLifeCycle>();

            await(intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Actor_Should_Implement_IActorLifeCycle()
        {
            var intface = _Factory.Build<IDummyInterface1>(new DisposableClass());
            intface.Should().BeAssignableTo<IActorCompleteLifeCycle>();

            await(intface as IActorLifeCycle).Stop();
        }

        [Fact]
        public async Task Actor_IActorLifeCycle_Stop_Should_Cancel_Actor_Thread()
        {
            //arrange
            var clas = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(clas);

            await intface.DoAsync();
            //act
            IActorCompleteLifeCycle disp = intface as IActorCompleteLifeCycle;

            await disp.Stop();

            //assert
            clas.CallingThread.IsAlive.Should().BeFalse();
        }

        [Fact]
        public async Task Actor_IActorLifeCycle_Stop_Should_Call_Proxified_Class_On_IAsyncDisposable()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);

            //act
            IActorCompleteLifeCycle disp = intface as IActorCompleteLifeCycle;

            await disp.Stop();

            //assert
            dispclass.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IActorLifeCycle_Stop()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);

            //act
            var task = intface.DoAsync();

            var disp = intface as IActorCompleteLifeCycle;

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

        [Fact]
        public async Task Actor_IActorLifeCycle_Stop_Should_Not_Cancel_Enqueued_Task()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);

            Task Taskrunning = intface.DoAsync(), Takenqueued = intface.DoAsync();
            Thread.Sleep(100);
            //act
            IActorCompleteLifeCycle disp = intface as IActorCompleteLifeCycle;

            await disp.Stop();
            await Takenqueued;

            //assert
            Takenqueued.IsCompleted.Should().BeTrue();
        }


        [Fact]
        public async Task Actor_IActorLifeCycle_Abort_Should_Cancel_Actor_Thread()
        {
            //arrange
            var clas = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(clas);

            await intface.DoAsync();
            //act
            var disp = intface as IActorCompleteLifeCycle;

            await disp.Abort();

            //assert
            clas.CallingThread.IsAlive.Should().BeFalse();
        }

        [Fact]
        public async Task Actor_IActorLifeCycle_Abort_Should_Call_Proxified_Class_On_IAsyncDisposable()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);

            //act
            var disp = intface as IActorCompleteLifeCycle;

            await disp.Abort();

            //assert
            dispclass.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_IActorLifeCycle_Abort_Should_Not_Cancel_RunningTask()
        {
            //arrange
            var taskCompletionSource = new TaskCompletionSource<object>();
            var progress = new Progress<int>(i => taskCompletionSource.TrySetResult(null));
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);
            var disp = intface as IActorCompleteLifeCycle;

            //act
            var Taskrunning = intface.DoAsync(progress);
            await taskCompletionSource.Task;
            await disp.Abort();
            await Taskrunning;
            Thread.Sleep(100);

            //assert
            Taskrunning.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_IActorLifeCycle_Abort_Should_Cancel_Enqueued_Task()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);

            intface.DoAsync();
            var Takenqueued = intface.DoAsync();
            Thread.Sleep(100);
            //act
            var disp = intface as IActorCompleteLifeCycle;

            await disp.Abort();

            //assert
            TaskCanceledException error = null;
            try
            {
                await Takenqueued;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            //assert
            error.Should().NotBeNull();
            Takenqueued.IsCanceled.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IActorLifeCycle_Abort()
        {
            //arrange
            var dispclass = new DisposableClass();
            var intface = _Factory.Build<IDummyInterface1>(dispclass);

            //act
            var task = intface.DoAsync();

            var disp = intface as IActorCompleteLifeCycle;

            await disp.Abort();

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
    }
}
