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

namespace EasyActor.Test
{
    [TestFixture]
    public class ActorContextTest
    {
        private Interface _Interface;
        private Class _Proxified;

        [SetUp]
        public void SetUp()
        {
            var factory = new ActorFactory();
            _Proxified = new Class();
            _Interface = factory.Build<Interface>(_Proxified);
        }


        [Test]
        public void TaskFactory_Should_Return_A_Valid_TaskFactory_With_A_None_Proxied_Object()
        {
            //arrange
            object randow = new object();
            var target = new ActorContext(randow);

            //act
            var res = target.TaskFactory;

            //assert
            res.Should().NotBeNull();
        }


        [Test]
        public void TaskFactory_Should_Return_A_Valid_TaskFactory_With_A_Proxied_Object()
        {
            //arrange
            var target = new ActorContext(_Proxified);

            //act
            var res = target.TaskFactory;

            //assert
            res.Should().NotBeNull();
        }


        [Test]
        public async Task TaskFactory_Should_Return_TaskFactory_Compatible_With_Proxy_Thread_ActorFactory_Context()
        {
            //arrange
            var target = new ActorContext(_Proxified);
            await _Interface.DoAsync();

            //act
            var res = target.TaskFactory;
            Thread tfthread = await res.StartNew(()=> Thread.CurrentThread);

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

            var target = new ActorContext(_Proxified);

            //act
            var res = target.TaskFactory;
            Thread tfthread = await res.StartNew(() => Thread.CurrentThread);

            //assert
            tfthread.Should().Be(UIMessageLoop.UIThread);

            UIMessageLoop.Stop();
        }


    }
}
