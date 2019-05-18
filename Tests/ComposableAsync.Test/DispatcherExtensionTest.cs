using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Core.Test
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
        public async Task GetAwaiter_After_Await_Switch_To_Fiber_Context()
        {
            var thread = Thread.CurrentThread;
            thread.Should().NotBeSameAs(_FiberThread);

            await _Fiber;

            thread = Thread.CurrentThread;
            thread.Should().BeSameAs(_FiberThread);
        }

        [Fact]
        public async Task GetAwaiter_Context_Is_Propagated()
        {
            await _Fiber;

            await Task.Yield();

            var thread = Thread.CurrentThread;
            thread.Should().BeSameAs(_FiberThread);
        }

        [Fact]
        public void GetAwaiter_IsComplete_Is_False()
        {
            var awaitable = _Dispatcher.GetAwaiter();
            awaitable.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void GetAwaiter_Dispatch_Action()
        {
            var action = Substitute.For<Action>();
            var awaitable = _Dispatcher.GetAwaiter();
            awaitable.OnCompleted(action);


            _Dispatcher.Received(1).Dispatch(Arg.Any<Action>());
            _Dispatcher.Received().Dispatch(action);
        }

        [Fact]
        public void Then_Returns_A_ComposedDispatcher()
        {
            var then = Substitute.For<IDispatcher>();
            var composed = _Dispatcher.Then(then);
            composed.Should().BeOfType<ComposedDispatcher>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Returns_A_ComposedCancellableDispatcher()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = Substitute.For<ICancellableDispatcher>();
            var composed = first.Then(then);
            composed.Should().BeOfType<ComposedCancellableDispatcher>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Throws_Exception_When_Other_Is_null()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = default(ICancellableDispatcher);
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Throws_Exception_When_This_Is_null()
        {
            var first = default(ICancellableDispatcher);
            var then = Substitute.For<ICancellableDispatcher>();
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Array_Returns_This_When_Array_Is_Empty()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = new ICancellableDispatcher[0];
            var composed = first.Then(then);
            composed.Should().Be(first);
        }

        [Fact]
        public void Then_ICancellableDispatcher_Array_Returns_A_ComposedCancellableDispatcher()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = Substitute.For<ICancellableDispatcher>();
            var composed = first.Then(then);
            composed.Should().BeOfType<ComposedCancellableDispatcher>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Array_Throws_Exception_When_Other_Is_null()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = default(ICancellableDispatcher[]);
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Array_Throws_Exception_When_This_Is_null()
        {
            var first = default(ICancellableDispatcher); ;
            var then = new ICancellableDispatcher[0];
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Enumerable_Returns_This_When_Array_Is_Empty()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = new List<ICancellableDispatcher>();
            var composed = first.Then(then);
            composed.Should().Be(first);
        }

        [Fact]
        public void Then_ICancellableDispatcher_Enumerable_Returns_A_ComposedCancellableDispatcher()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = new List<ICancellableDispatcher>()
            {
                Substitute.For<ICancellableDispatcher>()
            };
            var composed = first.Then(then);
            composed.Should().BeOfType<ComposedCancellableDispatcher>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Enumerable_Throws_Exception_When_Other_Is_null()
        {
            var first = Substitute.For<ICancellableDispatcher>();
            var then = default(IEnumerable<ICancellableDispatcher>);
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_ICancellableDispatcher_Enumerable_Throws_Exception_When_This_Is_null()
        {
            var first = default(ICancellableDispatcher);
            var then = new List<ICancellableDispatcher>();
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }
    }
}