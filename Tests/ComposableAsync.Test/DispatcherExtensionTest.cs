using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent;
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
            var awaiter = _Dispatcher.GetAwaiter();
            awaiter.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void GetAwaiter_Dispatch_Action()
        {
            var action = Substitute.For<Action>();
            var awaiter = _Dispatcher.GetAwaiter();
            awaiter.OnCompleted(action);

            _Dispatcher.Received(1).Dispatch(Arg.Any<Action>());
            _Dispatcher.Received().Dispatch(action);
        }

        [Fact]
        public async Task Await_Dispatch_Action()
        {
            SetUpDispatcherDispatch();
            await _Dispatcher;
            _Dispatcher.Received(1).Dispatch(Arg.Any<Action>());
        }

        [Fact]
        public async Task GetAwaiter_Rethrow_exception()
        {
            SetUpDispatcherDispatch();

            Func<Task> asyncFunction = async () => {
                await _Dispatcher;
                throw new ArgumentException();
            };

            await asyncFunction.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public void Then_Returns_A_ComposedDispatcher()
        {
            var then = Substitute.For<IDispatcher>();
            var composed = _Dispatcher.Then(then);
            composed.Should().BeOfType<ComposedDispatcher>();
        }

        [Fact]
        public void Then_Throws_Exception_When_Other_Is_null()
        {
            var first = Substitute.For<IDispatcher>();
            var then = default(IDispatcher);
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_Throws_Exception_When_This_Is_null()
        {
            var first = default(IDispatcher);
            var then = Substitute.For<IDispatcher>();
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_Array_Returns_This_When_Array_Is_Empty()
        {
            var first = Substitute.For<IDispatcher>();
            var then = new IDispatcher[0];
            var composed = first.Then(then);
            composed.Should().Be(first);
        }

        [Fact]
        public void Then_Array_Throws_Exception_When_Other_Is_null()
        {
            var first = Substitute.For<IDispatcher>();
            var then = default(IDispatcher[]);
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_Array_Throws_Exception_When_This_Is_null()
        {
            var first = default(IDispatcher); ;
            var then = new IDispatcher[0];
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_Enumerable_Returns_This_When_Array_Is_Empty()
        {
            var first = Substitute.For<IDispatcher>();
            var then = new List<IDispatcher>();
            var composed = first.Then(then);
            composed.Should().Be(first);
        }

        [Fact]
        public void Then_Enumerable_Returns_A_ComposedDispatcher()
        {
            var first = Substitute.For<IDispatcher>();
            var then = new List<IDispatcher>()
            {
                Substitute.For<IDispatcher>()
            };
            var composed = first.Then(then);
            composed.Should().BeOfType<ComposedDispatcher>();
        }

        [Fact]
        public void Then_Enumerable_Throws_Exception_When_Other_Is_null()
        {
            var first = Substitute.For<IDispatcher>();
            var then = default(IEnumerable<IDispatcher>);
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Then_Enumerable_Throws_Exception_When_This_Is_null()
        {
            var first = default(IDispatcher);
            var then = new List<IDispatcher>();
            Action @do = () => first.Then(then);
            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AsDelegatingHandler_Returns_A_DispatcherDelegatingHandler()
        {
            var dispatcher = Substitute.For<IDispatcher>();
            var handler = dispatcher.AsDelegatingHandler();

            handler.Should().BeOfType<DispatcherDelegatingHandler>();
        }

        [Fact]
        public void AsDelegatingHandler_Returns_Has_A_InnerHandler()
        {
            var dispatcher = Substitute.For<IDispatcher>();
            var handler = dispatcher.AsDelegatingHandler();

            handler.InnerHandler.Should().BeOfType<HttpClientHandler>();
        }

        private void SetUpDispatcherDispatch()
        {
            _Dispatcher.When(d => d.Dispatch(Arg.Any<Action>()))
                    .Do(x => ((Action) x[0])());
        }
    }
}