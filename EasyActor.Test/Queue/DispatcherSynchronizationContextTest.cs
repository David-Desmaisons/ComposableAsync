using System;
using System.Threading;
using EasyActor.Queue;
using FluentAssertions;
using Xunit;

namespace EasyActor.Test.Queue
{
     
    public class DispatcherSynchronizationContextTest
    {
        private readonly MonoThreadedQueueSynchronizationContext _Dispatcher;
        private readonly MonoThreadedQueue _Queue;

        public DispatcherSynchronizationContextTest()
        {
            _Queue = new MonoThreadedQueue(t => t.Priority = ThreadPriority.Highest);
            _Dispatcher = new  MonoThreadedQueueSynchronizationContext(_Queue);
        }

        [Fact]
        public void Constructor_Throw_Exception_On_Null_Queue() {
            MonoThreadedQueueSynchronizationContext res = null;
            Action Do =  () => res = new MonoThreadedQueueSynchronizationContext(null);

            Do.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext()
        {
            //arrange
            var cloned = _Dispatcher.CreateCopy();

            //assert
            cloned.Should().BeAssignableTo<MonoThreadedQueueSynchronizationContext>();
        }

        [Fact]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext_Conected_ToSameQueue()
        {
            //arrange
            var cloned = (MonoThreadedQueueSynchronizationContext)_Dispatcher.CreateCopy();

            //assert
            cloned.IsSame(_Dispatcher).Should().BeTrue();
        }
    }
}
