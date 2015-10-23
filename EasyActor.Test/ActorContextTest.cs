using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using EasyActor.TaskHelper;
using FluentAssertions;
using System.Threading;
using EasyActor.Test.WPFThreading;
using EasyActor.Queue;

namespace EasyActor.Test
{
    [TestFixture]
    public class ActorContextTest
    {
        private Interface _Interface;
        private Class _Proxified;
        private ActorContext _ActorContext;

        [SetUp]
        public void SetUp()
        {
            _ActorContext = new ActorContext();
            var factory = new ActorFactory();
            _Proxified = new Class();
            _Interface = factory.Build<Interface>(_Proxified);
        }


        [Test]
        public void TaskFactory_Should_Return_A_Valid_TaskFactory_With_A_None_Proxied_Object()
        {
            //arrange
            object random = new object();

            //act
            var res = _ActorContext.GetTaskFactory(random);

            //assert
            res.Should().NotBeNull();
        }

     


        [Test]
        public void TaskFactory_Should_Return_A_Valid_TaskFactory_With_A_Proxied_Object()
        {
            //act
            var res = _ActorContext.GetTaskFactory(_Proxified);

            //assert
            res.Should().NotBeNull();
        }

      


        [Test]
        public async Task TaskFactory_Should_Return_TaskFactory_Compatible_With_Proxy_Thread_ActorFactory_Context()
        {
            //arrange
            await _Interface.DoAsync();

            //act
            var res = _ActorContext.GetTaskFactory(_Proxified);
            Thread tfthread = await res.StartNew(()=> Thread.CurrentThread);

            //assert
            tfthread.Should().NotBeNull();
            tfthread.Should().Be(_Proxified.CallingThread);
        }

      
        [Test]
        public async Task TaskFactory_Should_Return_TaskFactory_Compatible_With_Proxy_Thread_SynchronizationContextFactory_Context()
        {
            //arrange
            var UIMessageLoop = new WPFThreadingHelper();
            UIMessageLoop.Start().Wait();

            var scf = UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());

            _Proxified = new Class();
            _Interface = scf.Build<Interface>(_Proxified);

            //act
            var res = _ActorContext.GetTaskFactory(_Proxified);
            Thread tfthread = await res.StartNew(() => Thread.CurrentThread);

            //assert
            tfthread.Should().Be(UIMessageLoop.UIThread);

            UIMessageLoop.Stop();
        }


        [Test]
        public void GetSynchronizationContext_Should_Return_Null_With_A_None_Proxied_Object()
        {
            //arrange
            object random = new object();

            //act
            var res = _ActorContext.GetSynchronizationContext(random);

            //assert
            res.Should().BeNull();
        }

        [Test]
        public async Task GetSynchronizationContext_Should_Return_SynchronizationContext_Compatible_With_Proxy_Thread_ActorFactory_Context()
        {
            //arrange
            await _Interface.DoAsync();

            //act
            var res = _ActorContext.GetSynchronizationContext(_Proxified);

            //assert
            res.Should().BeAssignableTo<MonoThreadedQueueSynchronizationContext>();
        }

        [Test]
        public void GetSynchronizationContext_Should_Return_SynchronizationContext_Compatible_With_Proxy_Thread_SynchronizationContextFactory_Context()
        {
            //arrange
            var UIMessageLoop = new WPFThreadingHelper();
            UIMessageLoop.Start().Wait();

            var scf = UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());

            _Proxified = new Class();
            _Interface = scf.Build<Interface>(_Proxified);

            //act
            var res = _ActorContext.GetSynchronizationContext(_Proxified);
            var uisc = UIMessageLoop.Dispatcher.Invoke(()=>  SynchronizationContext.Current);
         

            //assert
            res.Should().NotBeNull();
            res.Should().BeOfType(uisc.GetType());

            UIMessageLoop.Stop();
        }


    }
}
