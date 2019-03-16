using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using EasyActor.Options;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;

namespace EasyActor.Test
{     
    public class SharedThreadActorFactoryTest : IAsyncLifetime
    {
        private readonly IActorFactory _Factory;
        private IDummyInterface2 _Actor1;
        private IDummyInterface2 _Actor2;

        public SharedThreadActorFactoryTest()
        {
            _Factory = new FactoryBuilder().GetFactory(true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _Factory.DisposeAsync();
        }

        [Fact]
        public void Type_Should_Be_Shared()
        {
            _Factory.Type.Should().Be(ActorFactorType.Shared);
        }

        [Fact]
        public async Task All_Actors_Should_Run_On_Same_Thread()
        {
            //arrange
            var target1 = new DummyClass();
            var target2 = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target1);
            _Actor2 = _Factory.Build<IDummyInterface2>(target2);

            //act
            await _Actor1.DoAsync();
            await _Actor2.DoAsync();

            //assert
            target1.CallingThread.Should().Be(target2.CallingThread);
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            _Actor1 = await _Factory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await _Actor1.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.Should().Be(target.CallingThread);
        }

        [Fact]
        public async Task DisposeAsync_On_Factory_Should_Kill_Thread()
        {
            //arrange
            var target1 = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target1);
          
            //act
            await _Actor1.DoAsync();

            //assert
            await _Factory.DisposeAsync();

            Thread.Yield();
            target1.CallingThread.IsAlive.Should().BeFalse();
        }
    }
}
