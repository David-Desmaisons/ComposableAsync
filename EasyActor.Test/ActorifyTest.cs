using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using NUnit.Framework;

namespace EasyActor.Test
{
     [TestFixture]
    public class ActorifyTest
    {
        public interface Interface
        {
            Task SlowDoAsync();
            Task DoAsync();

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
                return Task.FromResult<object>(null);
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
                return Task.FromResult<object>(null);
            }


            public Task<int> ComputeAsync(int value)
            {
                CallingThread = Thread.CurrentThread;
                Thread.Sleep(1000);
                Done = true;
                return Task.FromResult<int>(value);
            }
        }
        private ActorFactory Actorify;

        public ActorifyTest()
        {       
        }

         [SetUp]
        public void TestUp()
        {
            Actorify = new ActorFactory();
        }


        [Test]
        public async Task DoAsync_ShouldRunOnOtherThread()
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
        public async Task DoAsync_ShouldRunOn_Always_Same_Thread()
        {
            var target = new Class();
            var intface = Actorify.Build<Interface>(target);
            await intface.DoAsync();

            var thread = target.CallingThread;

            await intface.DoAsync();

            target.CallingThread.Should().Be(thread);
        }

         [Test]
         public async Task Method_ShouldRunOn_Same_Thread_After_Await()
         {
             var target = new Class();
             var intface = Actorify.Build<Interface>(target);
            

             var res = await intface.DoAnRedoAsync();

             res.Item1.Should().Be(res.Item2);
         }
         

        [Test]
        public async Task Build_Delayed_Shoul_Be_Awaited()
        {
            var target = new Class();
            var intface = Actorify.Build<Interface>(target);
            await intface.SlowDoAsync();

            target.Done.Should().BeTrue();
        }

        [Test]
        public async Task Build_Delayed_Result_Should_Run_OnOtherTread()
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
        public void Build_No_Task_Should_Throw_Exception()
        {
            var intface = Actorify.Build<Interface>(new Class());
            Action Do = () => intface.Do();
            Do.ShouldThrow<NotSupportedException>();
        }
    }
}
