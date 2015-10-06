using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using EasyActor.TaskHelper;

namespace EasyActor.Test
{
     [TestFixture]
    public class ActorFactoryTest
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

            public Thread CallingThread { get;private set; }

            public bool Done { get; set; }
            public Task DoAsync()
            {
                CallingThread = Thread.CurrentThread;
                Done = true;
                return TaskBuilder.GetCompleted();
            }


            public async Task<Tuple<Thread,Thread>> DoAnRedoAsync()
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
                return TaskBuilder.GetCompleted();
            }


            public Task<int> ComputeAsync(int value)
            {
                CallingThread = Thread.CurrentThread;
                Thread.Sleep(1000);
                Done = true;
                return Task.FromResult<int>(value);
            }
        }

        public class DisposbaleClass : Interface1, IDisposable
        {
            public DisposbaleClass()
            {
                IsDisposed = false;
            }

            public bool IsDisposed { get; private set; }

            public Task DoAsync()
            {
                return Task.FromResult<object>(null);
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }


        private ActorFactory Actorify;

        public ActorFactoryTest()
        {       
        }

         [SetUp]
        public void TestUp()
        {
            Actorify = new ActorFactory();
        }


        [Test]
        public async Task Method_Should_Run_On_Separated_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new Class();
            var intface = Actorify.Build<Interface>(target);
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }

         [Test]
        public async Task Method_Should_Always_Run_On_Same_Thread()
        {
            var target = new Class();
            var intface = Actorify.Build<Interface>(target);
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
             var intface1 = Actorify.Build<Interface>(target1);
             var intface2 = Actorify.Build<Interface>(target2);

             //act
             await intface1.DoAsync();
             await intface2.DoAsync();

             //assert
             target1.CallingThread.Should().NotBe(target2.CallingThread);
         }

         [Test]
         public async Task All_Actors_Should_Run_On_Same_Thread_When_SharedThread_Is_True()
         {

             var factory = new ActorFactory(SharedThread:true);
             //arrange
             var target1 = new Class();
             var target2 = new Class();
             var intface1 = factory.Build<Interface>(target1);
             var intface2 = factory.Build<Interface>(target2);

             //act
             await intface1.DoAsync();
             await intface2.DoAsync();

             //assert
             target1.CallingThread.Should().Be(target2.CallingThread);
         }

         [Test]
         public async Task Method_Should_Run_On_Same_Thread_After_Await()
         {
             var target = new Class();
             var intface = Actorify.Build<Interface>(target);
            

             var res = await intface.DoAnRedoAsync();

             res.Item1.Should().Be(res.Item2);
         }
         

        [Test]
        public async Task Task_returned_By_Method_Should_Be_Awaited()
        {
            var target = new Class();
            var intface = Actorify.Build<Interface>(target);
            await intface.SlowDoAsync();

            target.Done.Should().BeTrue();
        }

        [Test]
        public async Task Method_With_Task_T_Should_Run_On_Separated_Tread()
        {
            var current = Thread.CurrentThread;
            var target = new Class();
            var intface = Actorify.Build<Interface>(target);
            var result = await intface.ComputeAsync(25);

            result.Should().Be(25);
            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }


         [Test]
        public void Method_returning_void_Task_Should_Throw_Exception()
        {
            var intface = Actorify.Build<Interface>(new Class());
            Action Do = () => intface.Do();
            Do.ShouldThrow<NotSupportedException>();
        }

         [Test]
         public void Actor_Should_Be_Disposable()
         {
             var intface = Actorify.Build<Interface>(new Class()) as IDisposable;

             intface.Should().NotBeNull();
         }

         [Test]
         public async Task Actor__Dispose_Should_Cancel_Thread_Loop()
         {
             //arrange
             var dispclass = new DisposbaleClass();
             var intface = Actorify.Build<Interface1>(dispclass);

             //act
             await intface.DoAsync();

             var disp = intface as IDisposable;

             disp.Dispose();

             Thread.Sleep(1500);
             //assert
             dispclass.IsDisposed.Should().BeTrue();
         }
    }
}
