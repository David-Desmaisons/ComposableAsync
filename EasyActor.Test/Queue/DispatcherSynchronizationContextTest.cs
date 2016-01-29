using System;
using NUnit.Framework;
using FluentAssertions;
using EasyActor.Queue;
using System.Threading;

namespace EasyActor.Test
{
    [TestFixture]
    public class DispatcherSynchronizationContextTest
    {
        private MonoThreadedQueueSynchronizationContext _Dispatcher;
        private MonoThreadedQueue _Queue;
        [SetUp]
        public void SetUp()
        {
            _Queue = new MonoThreadedQueue(t => t.Priority = ThreadPriority.Highest);
            _Dispatcher = new  MonoThreadedQueueSynchronizationContext(_Queue);
        }

        [Test]
        public void Constructor_Throw_Exception_On_Null_Queue() {
            MonoThreadedQueueSynchronizationContext res = null;
            Action Do =  () => res = new MonoThreadedQueueSynchronizationContext(null);

            Do.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Post_Should_Run_On_Queue_Thread()
        {
            //arrange
            Thread queuethread=null;
            _Queue.Enqueue(() => queuethread = Thread.CurrentThread);
            
            //act
            Thread postthread = null;
            SendOrPostCallback post = (o) => { postthread = Thread.CurrentThread; };
            _Dispatcher.Post(post, null);
            Thread.Sleep(500);

            //assert
            postthread.Should().Be(queuethread);
        }

        [Test]
        public void Send_Should_Run_On_Queue_Thread()
        {
            //arrange
            Thread queuethread = null;
            _Queue.Enqueue(() => queuethread = Thread.CurrentThread);

            //act
            Thread postthread = null;
            SendOrPostCallback post = (o) => { postthread = Thread.CurrentThread; };
            _Dispatcher.Send(post, null);

            //assert
            postthread.Should().Be(queuethread);
        }

        [Test]
        public void Send_Should_Run_Immediately_On_Queue_Thread()
        {
            //arrange
            Thread queuethread = null;
            _Queue.Enqueue(() => queuethread = Thread.CurrentThread);

            //act
            Thread postthread = null;
            SendOrPostCallback post = (o) => { postthread = Thread.CurrentThread; };        
            _Queue.Enqueue(() =>   _Dispatcher.Send(post, null));

            //assert
            postthread.Should().Be(queuethread);
        }

        [Test]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext()
        {
            //arrange
            var cloned = _Dispatcher.CreateCopy();

            //assert
            cloned.Should().BeAssignableTo<MonoThreadedQueueSynchronizationContext>();
        }

        [Test]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext_Conected_ToSameQueue()
        {
            //arrange
            var cloned = (MonoThreadedQueueSynchronizationContext)_Dispatcher.CreateCopy();

            //assert
            cloned.IsSame(_Dispatcher).Should().BeTrue();
        }
    }
}
