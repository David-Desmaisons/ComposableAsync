using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using EasyActor.Queue;
using System.Threading;

namespace EasyActor.Test
{
    [TestFixture]
    public class DispatcherSynchronizationContextTest
    {
        private DispatcherSynchronizationContext _Dispatcher;
        private MonoThreadedQueue _Queue;
        [SetUp]
        public void SetUp()
        {
            _Queue = new MonoThreadedQueue(Priority.Highest);
            _Dispatcher = new  DispatcherSynchronizationContext(_Queue);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throw_Exception_On_Null_Queue()
        {
            new DispatcherSynchronizationContext(null);
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
            cloned.Should().BeAssignableTo<DispatcherSynchronizationContext>();
        }

        [Test]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext_Conected_ToSameQueue()
        {
            //arrange

            var cloned = (DispatcherSynchronizationContext)_Dispatcher.CreateCopy();

            //assert
            cloned.IsSame(_Dispatcher).Should().BeTrue();
        }
    }
}
