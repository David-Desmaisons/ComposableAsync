﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using Concurrent.Test.TestHelper;
using EasyActor.Factories;
using EasyActor.Options;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;

namespace EasyActor.Test
{

    public class SynchronizationContextFactoryTest : IDisposable
    {
        private readonly WpfThreadingHelper _UiMessageLoop;
        private readonly SynchronizationContextFactory _Factory;

        public SynchronizationContextFactoryTest()
        {
            _UiMessageLoop = new WpfThreadingHelper();
            _UiMessageLoop.Start().Wait();

            _Factory = _UiMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());
        }

        public void Dispose()
        {
            _UiMessageLoop.Dispose();
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
            Do.Should().Throw<ArgumentNullException>();
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
            obj.CallingThread.Should().Be(_UiMessageLoop.UiThread);
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
            obj.CallingThread.Should().Be(_UiMessageLoop.UiThread);
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
        public async Task Build_Should_Throw_Exception_IsSamePOCO_HasBeenUsedWithOtherFactory()
        {
            var target = new DummyClass();
            var factory = new ActorFactory();
            var intface = factory.Build<IDummyInterface2>(target);

            Action Do = () => _Factory.Build<IDummyInterface2>(target);

            Do.Should().Throw<ArgumentException>().And.Message.Should().Contain("Standard");

            await factory.DisposeAsync();
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
