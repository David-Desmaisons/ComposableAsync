using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using System.Threading;

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
            var target = new Class();
            var intface = _TaskPoolActorFactory.Build<Interface>(target);
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
            var target = new Class();
            var intface = _TaskPoolActorFactory.Build<Interface>(target);
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
            var target1 = new Class();
            var target2 = new Class();
            var intface1 = _TaskPoolActorFactory.Build<Interface>(target1);
            var intface2 = _TaskPoolActorFactory.Build<Interface>(target2);

            //act
            await Task.WhenAll(intface1.SlowDoAsync(), intface2.SlowDoAsync());

            //assert
            target1.CallingThread.Should().NotBe(target2.CallingThread);
        }

        [Test]
        public async Task BuildAsync_Should_Call_Constructor_On_ThreadPoolThread()
        {
            var current = Thread.CurrentThread;
            Class target = null;
            Interface intface = await _TaskPoolActorFactory.BuildAsync<Interface>(() => { target = new Class(); return target; });
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.Should().Be(target.CallingThread);
            target.CallingConstructorThread.IsThreadPoolThread.Should().BeTrue();
        }
    }
}
