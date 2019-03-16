using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Concurrent.Test
{
    public class DispatcherExtensionTest : IAsyncLifetime
    {
        private readonly IMonoThreadFiber _Fiber;
        private readonly IDispatcher _Dispatcher = Substitute.For<IDispatcher>();
        private Thread _FiberThread;

        public DispatcherExtensionTest()
        {
            _Fiber = Fiber.CreateMonoThreadedFiber();
        }

        public async Task InitializeAsync()
        {
            _FiberThread = await _Fiber.Enqueue(() => Thread.CurrentThread);
        }

        public Task DisposeAsync()
        {
            return _Fiber.DisposeAsync();
        }

        [Fact]
        public async Task SwitchToContext_After_Await_Switch_To_Fiber_Context()
        {
            var thread = Thread.CurrentThread;
            thread.Should().NotBeSameAs(_FiberThread);

            await _Fiber.SwitchToContext();

            thread = Thread.CurrentThread;
            thread.Should().BeSameAs(_FiberThread);
        }

        [Fact]
        public async Task SwitchToContext_Context_Is_Propagated()
        {
            await _Fiber.SwitchToContext();

            await Task.Yield();

            var thread = Thread.CurrentThread;
            thread.Should().BeSameAs(_FiberThread);
        }

        [Fact]
        public void SwitchToContext_GetAwaibale_IsComplete_Is_False()
        {
            var awaitable = _Dispatcher.SwitchToContext().GetAwaiter();
            awaitable.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void SwitchToContext_GetAwaibale_Dispatch_Action()
        {
            var action = Substitute.For<Action>();
            var awaitable = _Dispatcher.SwitchToContext().GetAwaiter();
            awaitable.OnCompleted(action);


            _Dispatcher.Received(1).Dispatch(Arg.Any<Action>());
            _Dispatcher.Received().Dispatch(action);
        }
    }
}
