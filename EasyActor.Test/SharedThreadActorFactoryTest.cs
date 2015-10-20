using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using EasyActor.TaskHelper;

namespace EasyActor.Test
{
    [TestFixture]
    public class SharedThreadActorFactoryTest
    {
        private SharedThreadActorFactory _Factory;
        [SetUp]
        public void setUp()
        {
            _Factory = new SharedThreadActorFactory();
        }

        [Test]
        public void Type_Should_Be_Shared()
        {
            _Factory.Type.Should().Be(ActorFactorType.Shared);
        }


        [Test]
        public async Task All_Actors_Should_Run_On_Same_Thread()
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
            target1.CallingThread.Should().Be(target2.CallingThread);
        }

        [Test]
        public async Task Stop_Should_Kill_Thread()
        {
            //arrange
            var target1 = new Class();
            var intface1 = _Factory.Build<Interface>(target1);
          

            //act
            await intface1.DoAsync();

            //assert
            await _Factory.Stop();
            target1.CallingThread.IsAlive.Should().BeFalse();
        }

        [Test]
        public async Task Abort_Should_Kill_Thread()
        {
            //arrange
            var target1 = new Class();
            var intface1 = _Factory.Build<Interface>(target1);


            //act
            await intface1.DoAsync();

            //assert
            await _Factory.Abort();
            target1.CallingThread.IsAlive.Should().BeFalse();
        }

        [Test]
        public async Task Stop_Should_Call_Actor_IAsyncDisposable_DisposeAsync_Thread()
        {
            //arrange
            var target1 = new DisposableClass();
            var intface1 = _Factory.Build<Interface1>(target1);


            //act
            await intface1.DoAsync();

            //assert
            await _Factory.Stop();
            target1.IsDisposed.Should().BeTrue();
        }

        [Test]
        public async Task Abort_Should_Call_Actor_IAsyncDisposable_DisposeAsync_Thread()
        {
            //arrange
            var target1 = new DisposableClass();
            var intface1 = _Factory.Build<Interface1>(target1);


            //act
            await intface1.DoAsync();

            //assert
            await _Factory.Abort();
            target1.IsDisposed.Should().BeTrue();
        }
    }
}
