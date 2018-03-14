using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using EasyActor.Factories;
using EasyActor.Test.TestInfra.DummyClass;
using EasyActor.Test.TestInfra.WPFThreading;
using Xunit;

namespace EasyActor.Test
{
     
    public class SynchronizationContextFactoryTest : IDisposable
    {
        private readonly WpfThreadingHelper _UIMessageLoop;
        private readonly SynchronizationContextFactory _Factory;

        public SynchronizationContextFactoryTest()
        {
            _UIMessageLoop = new WpfThreadingHelper();
            _UIMessageLoop.Start().Wait();

            _Factory = _UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());
        }

        public void Dispose()
        {
            _UIMessageLoop.Dispose();
        }

        [Fact]
        public void Type_Should_Be_InCurrentContext()
        {
            _Factory.Type.Should().Be(ActorFactorType.InCurrentContext);
        }

        [Fact]
        public void Creating_SynchronizationContextFactory_WithoutContext_ThrowException() 
        {
            SynchronizationContext.SetSynchronizationContext(null);
            SynchronizationContextFactory res = null;
            Action Do = () => res = new SynchronizationContextFactory();
            Do.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task Proxy_Method_WithoutResult_Should_Be_Called()
        {
            //arrange
            var obj = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(obj);

            //act
            await actor.DoAsync();

            //assert
            obj.Done.Should().BeTrue();
        }

        [Fact]
        public async Task Proxy_Method_WithResult_Should_Called()
        {
            //arrange
            var obj = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(obj);

            //act
            var res = await actor.ComputeAsync(5);

            //assert
            obj.Done.Should().BeTrue();
            res.Should().Be(5);
        }

        [Fact]
        public async Task Proxy_Method_WithoutResult_Should_Dispatch_On_Captured_UI_Thread()
        {
            //arrange
            var obj = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(obj);

            //act
            await actor.DoAsync();

            //assert
            obj.CallingThread.Should().Be(_UIMessageLoop.UiThread);
        }

        [Fact]
        public async Task Proxy_Method_WithResult_Should_Dispatch_On_Captured_UI_Thread()
        {
            //arrange
            var obj = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(obj);

            //act
            await actor.ComputeAsync(5);

            //assert
            obj.CallingThread.Should().Be(_UIMessageLoop.UiThread);
        }

        [Fact]
        public void Build_Should_CreateSameInterface_ForSamePOCO()
        {
            var target = new DummyClass();
            var intface = _Factory.Build<IDummyInterface2>(target);
            var intface2 = _Factory.Build<IDummyInterface2>(target);

            intface.Should().BeSameAs(intface2);
        }

        [Fact]
        public void Build_Should_Throw_Exception_IsSamePOCO_HasBeenUsedWithOtherFactory()
        {
            var target = new DummyClass();
            var Factory = new ActorFactory();
            var intface = Factory.Build<IDummyInterface2>(target);

            Action Do = () => _Factory.Build<IDummyInterface2>(target);

            Do.ShouldThrow<ArgumentException>().And.Message.Should().Contain("Standard");

            (intface as IActorLifeCycle)?.Stop().Wait();
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            var intface = await _Factory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await intface.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.Should().Be(target.CallingThread);
        }
    }
}
