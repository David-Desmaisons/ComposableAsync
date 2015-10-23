using System;
using System.Threading.Tasks;
using System.Threading;

using FluentAssertions;
using NUnit.Framework;

using EasyActor.TaskHelper;

namespace EasyActor.Test
{
     [TestFixture]
    public class ActorFactoryTest
    {    

        private ActorFactory _Factory;

        [SetUp]
        public void TestUp()
        {
            _Factory = new ActorFactory();
        }

        [Test]
        public void Type_Should_Be_Standard()
        {
            _Factory.Type.Should().Be(ActorFactorType.Standard);
        }

        


        [Test]
        public async Task Method_Should_Run_On_Separated_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new Class();
            var intface = _Factory.Build<Interface>(target);
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }

         [Test]
        public async Task Method_Should_Always_Run_On_Same_Thread()
        {
            var target = new Class();
            var intface = _Factory.Build<Interface>(target);
            await intface.DoAsync();

            var thread = target.CallingThread;

            await intface.DoAsync();

            target.CallingThread.Should().Be(thread);
        }

         [Test]
         public async Task Each_Actor_Should_Run_On_Separated_Thread_When_Shared_Thread_Is_False()
         {
             //arrange
             var target1 = new Class();
             var target2 = new Class();
             var intface1 = _Factory.Build<Interface>(target1);
             var intface2 = _Factory.Build<Interface>(target2);

             //act
             await intface1.DoAsync();
             await intface2.DoAsync();

             //assert
             target1.CallingThread.Should().NotBe(target2.CallingThread);
         }

      

         [Test]
         public async Task Method_Should_Run_On_Same_Thread_After_Await()
         {
             var target = new Class();
             var intface = _Factory.Build<Interface>(target);
            

             var res = await intface.DoAnRedoAsync();

             res.Item1.Should().Be(res.Item2);
         }
         

        [Test]
        public async Task Task_returned_By_Method_Should_Be_Awaited()
        {
            var target = new Class();
            var intface = _Factory.Build<Interface>(target);
            await intface.SlowDoAsync();

            target.Done.Should().BeTrue();
        }

        [Test]
        public async Task Method_With_Task_T_Should_Run_On_Separated_Tread()
        {
            var current = Thread.CurrentThread;
            var target = new Class();
            var intface = _Factory.Build<Interface>(target);
            var result = await intface.ComputeAsync(25);

            result.Should().Be(25);
            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }


         [Test]
        public void Method_returning_void_Task_Should_Throw_Exception()
        {
            var intface = _Factory.Build<Interface>(new Class());
            Action Do = () => intface.Do();
            Do.ShouldThrow<NotSupportedException>();
        }

         [Test]
         public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
         {
             var current = Thread.CurrentThread;
             Class target = null;
             Interface intface = await _Factory.BuildAsync<Interface>(() => { target = new Class(); return target; });
             await intface.DoAsync();

             target.Done.Should().BeTrue();
             target.CallingConstructorThread.Should().NotBe(current);
             target.CallingConstructorThread.Should().Be(target.CallingThread);
         }



         [Test]
         public void Actor_Should_Be_Implement_IActorLifeCycle_Even_If_Wrapped_Mot()
         {
             var intface = _Factory.Build<Interface>(new Class()) as IActorLifeCycle;

             intface.Should().NotBeNull();
         }

         [Test]
         public void Actor_Should_Be_Implement_IActorLifeCycle()
         {
             var intface = _Factory.Build<Interface1>(new DisposableClass()) as IActorLifeCycle;

             intface.Should().NotBeNull();
         }



         [Test]
         public async Task Actor_IActorLifeCycle_Stop_Should_Cancel_Actor_Thread()
         {
             
             //arrange
             var clas = new Class();
             var intface = _Factory.Build<Interface>(clas);

             await intface.DoAsync();
             //act
             IActorLifeCycle disp = intface as IActorLifeCycle;

             await disp.Stop();

             //assert
             clas.CallingThread.IsAlive.Should().BeFalse();
         }

         [Test]
         public async Task Actor_IActorLifeCycle_Stop_Should_Call_Proxified_Class_On_IAsyncDisposable()
         {
             //arrange
             var dispclass = new DisposableClass();
             var intface = _Factory.Build<Interface1>(dispclass);

             //act
             IActorLifeCycle disp = intface as IActorLifeCycle;

             await disp.Stop();

             //assert
             dispclass.IsDisposed.Should().BeTrue();
         }

         [Test]
         public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IActorLifeCycle_Stop()
         {
             //arrange
             var dispclass = new DisposableClass();
             var intface = _Factory.Build<Interface1>(dispclass);

             //act
             var task = intface.DoAsync();

             var disp = intface as IActorLifeCycle;

             await disp.Stop();

             TaskCanceledException error = null;
             Task canc = null;
             try
             {
                 canc=intface.DoAsync();
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
             var intface = _Factory.Build<Interface1>(dispclass);

             Task Taskrunning = intface.DoAsync(), Takenqueued = intface.DoAsync();
             Thread.Sleep(100);
             //act
             IActorLifeCycle disp = intface as IActorLifeCycle;

             await disp.Stop(); 
             await Takenqueued;

             //assert
             Takenqueued.IsCompleted.Should().BeTrue();
         }


         [Test]
         public async Task Actor_IActorLifeCycle_Abort_Should_Cancel_Actor_Thread()
         {

             //arrange
             var clas = new Class();
             var intface = _Factory.Build<Interface>(clas);

             await intface.DoAsync();
             //act
             IActorLifeCycle disp = intface as IActorLifeCycle;

             await disp.Abort();

             //assert
             clas.CallingThread.IsAlive.Should().BeFalse();
         }



         [Test]
         public async Task Actor_IActorLifeCycle_Abort_Should_Call_Proxified_Class_On_IAsyncDisposable()
         {
             //arrange
             var dispclass = new DisposableClass();
             var intface = _Factory.Build<Interface1>(dispclass);

             //act
             IActorLifeCycle disp = intface as IActorLifeCycle;

             await disp.Abort();

             //assert
             dispclass.IsDisposed.Should().BeTrue();
         }

         [Test]
         public async Task Actor_IActorLifeCycle_Abort_Should_Not_Cancel_RunningTask()
         {
             //arrange
             var dispclass = new DisposableClass();
             var intface = _Factory.Build<Interface1>(dispclass);
             IActorLifeCycle disp = intface as IActorLifeCycle;

             //act
             var Taskrunning = intface.DoAsync();
             await disp.Abort();
             await Taskrunning;
             Thread.Sleep(100);
 
             //assert
             Taskrunning.IsCompleted.Should().BeTrue();
         }

         [Test]
         public async Task Actor_IActorLifeCycle_Abort_Should_Cancel_Enqueued_Task()
         {
             //arrange
             var dispclass = new DisposableClass();
             var intface = _Factory.Build<Interface1>(dispclass);

             Task Taskrunning = intface.DoAsync(), Takenqueued = intface.DoAsync();
             Thread.Sleep(100);
             //act
             IActorLifeCycle disp = intface as IActorLifeCycle;

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

         [Test]
         public async Task Actor_Should_Return_Cancelled_Task_On_Any_Method_AfterCalling_IActorLifeCycle_Abort()
         {
             //arrange
             var dispclass = new DisposableClass();
             var intface = _Factory.Build<Interface1>(dispclass);

             //act
             var task = intface.DoAsync();

             var disp = intface as IActorLifeCycle;

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
