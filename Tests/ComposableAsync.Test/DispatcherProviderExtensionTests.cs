using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Core.Test
{
    public class DispatcherProviderExtensionTests
    {
        private readonly IDispatcherProvider _DispatcherProvider;

        public DispatcherProviderExtensionTests()
        {
            _DispatcherProvider = Substitute.For<IDispatcherProvider>();
        }

        [Fact]
        public void GetAssociatedDispatcher_Returns_Dispatcher_When_Not_Null()
        {
            var expected = Substitute.For<IDispatcher>();
            _DispatcherProvider.Dispatcher.Returns(expected);

            var dispatcher = _DispatcherProvider.GetAssociatedDispatcher();
            dispatcher.Should().Be(expected);
        }

        [Fact]
        public void GetAssociatedDispatcher_Returns_Null_Dispatcher_When_Null()
        {
            _DispatcherProvider.Dispatcher.Returns(default(IDispatcher));

            var dispatcher = _DispatcherProvider.GetAssociatedDispatcher();
            dispatcher.Should().Be(NullDispatcher.Instance);
        }

        [Fact]
        public void GetAssociatedDispatcher_Returns_Null_Dispatcher_When_DispatcherProvider_Is_Null()
        {
            var dispatcher = default(IDispatcherProvider).GetAssociatedDispatcher();
            dispatcher.Should().Be(NullDispatcher.Instance);
        }
    }
}
