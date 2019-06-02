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
        private readonly ICancellableDispatcher _CancellableDispatcher;
        private readonly ICancellableDispatcher _CancellableDispatcher2;
        private readonly ICancellableDispatcher _CancellableDispatcher3;

        public ProxyFactoryBuilderTest()
        {
            _ProxyFactoryBuilder = new ProxyFactoryBuilder();
            _CancellableDispatcher = Substitute.For<ICancellableDispatcher, IAsyncDisposable>();
            _CancellableDispatcher2 = Substitute.For<ICancellableDispatcher, IAsyncDisposable>();
            _CancellableDispatcher3 = Substitute.For<ICancellableDispatcher, IAsyncDisposable>();
        }

        [Fact]
        public void GetManagedProxyFactory_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_CancellableDispatcher);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetManagedProxyFactory_Returns_ProxyFactory_That_Dispose_Dispatcher()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_CancellableDispatcher);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_CancellableDispatcher).Received(1).DisposeAsync();
        }

        [Fact]
        public void GetManagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetManagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory_That_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_CancellableDispatcher).Received(1).DisposeAsync();
            await ((IAsyncDisposable)_CancellableDispatcher2).Received(1).DisposeAsync();
        }

        [Fact]
        public void GetManagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2, _CancellableDispatcher3);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetManagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory_That_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetManagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2, _CancellableDispatcher3);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_CancellableDispatcher).Received(1).DisposeAsync();
            await ((IAsyncDisposable)_CancellableDispatcher2).Received(1).DisposeAsync();
            await ((IAsyncDisposable)_CancellableDispatcher3).Received(1).DisposeAsync();
        }

        [Fact]
        public void GetUnmanagedProxyFactory_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_CancellableDispatcher);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetUnmanagedProxyFactory_Returns_ProxyFactory_That_DoesNot_Dispose_Dispatcher()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_CancellableDispatcher);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_CancellableDispatcher).DidNotReceive().DisposeAsync();
        }

        [Fact]
        public void GetUnmanagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetUnmanagedProxyFactory_2_CancellableDispatcher_Returns_ProxyFactory_That_DoesNot_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_CancellableDispatcher).DidNotReceive().DisposeAsync();
            await ((IAsyncDisposable)_CancellableDispatcher2).DidNotReceive().DisposeAsync();
        }

        [Fact]
        public void GetUnmanagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2, _CancellableDispatcher3);

            factory.Should().BeAssignableTo<ProxyFactory>();
        }

        [Fact]
        public async Task GetUnmanagedProxyFactory_CancellableDispatchers_Returns_ProxyFactory_That_DoesNot_Dispose_Dispatchers()
        {
            var factory = _ProxyFactoryBuilder.GetUnmanagedProxyFactory(_CancellableDispatcher, _CancellableDispatcher2, _CancellableDispatcher3);

            await factory.DisposeAsync();

            await ((IAsyncDisposable)_CancellableDispatcher).DidNotReceive().DisposeAsync();
            await ((IAsyncDisposable)_CancellableDispatcher2).DidNotReceive().DisposeAsync();
            await ((IAsyncDisposable)_CancellableDispatcher3).DidNotReceive().DisposeAsync();
        }
    }
}
