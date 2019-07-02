using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Factory.Test
{
    public class ProxyFactoryBuilderTest
    {
        private readonly ProxyFactoryBuilder _ProxyFactoryBuilder;
        private readonly IDispatcher _Dispatcher;
        private readonly IDispatcher _Dispatcher2;
        private readonly IDispatcher _Dispatcher3;

        public ProxyFactoryBuilderTest()
        {
            _ProxyFactoryBuilder = new ProxyFactoryBuilder();
            _Dispatcher = Substitute.For<IDispatcher, IAsyncDisposable>();
            _Dispatcher2 = Substitute.For<IDispatcher, IAsyncDisposable>();
            _Dispatcher3 = Substitute.For<IDispatcher, IAsyncDisposable>();
        }

        [Fact]
        public void GetManagedProxyFactory_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_Dispatcher);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetManagedProxyFactory_Returns_ProxyFactory_That_Dispose_Dispatcher()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_Dispatcher);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_Dispatcher).Received(1).DisposeAsync();
        }

        [Fact]
        public void GetManagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_Dispatcher, _Dispatcher2);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetManagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory_That_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_Dispatcher, _Dispatcher2);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_Dispatcher).Received(1).DisposeAsync();
            await ((IAsyncDisposable)_Dispatcher2).Received(1).DisposeAsync();
        }

        [Fact]
        public void GetManagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_Dispatcher, _Dispatcher2, _Dispatcher3);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetManagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory_That_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_Dispatcher, _Dispatcher2, _Dispatcher3);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_Dispatcher).Received(1).DisposeAsync();
            await ((IAsyncDisposable)_Dispatcher2).Received(1).DisposeAsync();
            await ((IAsyncDisposable)_Dispatcher3).Received(1).DisposeAsync();
        }

        [Fact]
        public void GetUnmanagedProxyFactory_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_Dispatcher);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetUnmanagedProxyFactory_Returns_ProxyFactory_That_DoesNot_Dispose_Dispatcher()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_Dispatcher);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_Dispatcher).DidNotReceive().DisposeAsync();
        }

        [Fact]
        public void GetUnmanagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_Dispatcher, _Dispatcher2);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetUnmanagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory_That_DoesNot_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_Dispatcher, _Dispatcher2);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_Dispatcher).DidNotReceive().DisposeAsync();
            await ((IAsyncDisposable)_Dispatcher2).DidNotReceive().DisposeAsync();
        }

        [Fact]
        public void GetUnmanagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_Dispatcher, _Dispatcher2, _Dispatcher3);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetUnmanagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory_That_DoesNot_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_Dispatcher, _Dispatcher2, _Dispatcher3);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_Dispatcher).DidNotReceive().DisposeAsync();
            await ((IAsyncDisposable)_Dispatcher2).DidNotReceive().DisposeAsync();
            await ((IAsyncDisposable)_Dispatcher3).DidNotReceive().DisposeAsync();
        }
    }
}
