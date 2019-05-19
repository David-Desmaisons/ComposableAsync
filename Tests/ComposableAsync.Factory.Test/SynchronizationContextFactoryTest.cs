﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Factory.Test.TestInfra.DummyClass;
using Concurrent.Test.Helper;
using FluentAssertions;
using Xunit;

namespace ComposableAsync.Factory.Test
{
    public class SynchronizationContextFactoryTest : IDisposable
    {
        private readonly WpfThreadingHelper _UiMessageLoop;
        private readonly IProxyFactory _Factory;

        public SynchronizationContextFactoryTest()
        {
            _UiMessageLoop = new WpfThreadingHelper();
            _UiMessageLoop.Start().Wait();

            _Factory = _UiMessageLoop.Dispatcher.Invoke(() => _ProxyFactoryBuilder.GetInContextActorFactory());
        }

        public void Dispose()
        {
            _UiMessageLoop.Dispose();
        }

        private static readonly ProxyFactoryBuilder _ProxyFactoryBuilder = new ProxyFactoryBuilder();

        [Fact]
        public void Creating_SynchronizationContextFactory_WithoutContext_ThrowException() 
        {
            SynchronizationContext.SetSynchronizationContext(null);
            Action Do = () => _ProxyFactoryBuilder.GetInContextActorFactory();
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
