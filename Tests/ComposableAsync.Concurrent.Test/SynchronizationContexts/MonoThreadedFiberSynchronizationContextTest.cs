﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.Fibers;
using ComposableAsync.Concurrent.SynchronizationContexts;
using FluentAssertions;
using Xunit;

namespace ComposableAsync.Concurrent.Test.SynchronizationContexts
{

    public class MonoThreadedFiberSynchronizationContextTest : IAsyncLifetime
    {
        private readonly MonoThreadedFiberSynchronizationContext _Dispatcher;
        private readonly MonoThreadedFiber _Fiber;

        public MonoThreadedFiberSynchronizationContextTest()
        {
            _Fiber = new MonoThreadedFiber(t => t.Priority = ThreadPriority.Highest);
            _Dispatcher = new MonoThreadedFiberSynchronizationContext(_Fiber);
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => _Fiber.DisposeAsync();

        [Fact]
        public void Constructor_Throw_Exception_On_Null_Queue()
        {
            MonoThreadedFiberSynchronizationContext res = null;
            Action @do = () => res = new MonoThreadedFiberSynchronizationContext(null);

            @do.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Post_Should_Run_On_Queue_Thread()
        {
            //arrange
            Thread queuethread = null;
            _Fiber.Enqueue(() => queuethread = Thread.CurrentThread);

            //act
            Thread postthread = null;
            SendOrPostCallback post = (o) => { postthread = Thread.CurrentThread; };
            _Dispatcher.Post(post, null);
            Thread.Sleep(500);

            //assert
            postthread.Should().Be(queuethread);
        }

        [Fact]
        public void Send_Should_Run_On_Queue_Thread()
        {
            //arrange
            Thread queuethread = null;
            _Fiber.Enqueue(() => queuethread = Thread.CurrentThread);

            //act
            Thread postthread = null;
            SendOrPostCallback post = (o) => { postthread = Thread.CurrentThread; };
            _Dispatcher.Send(post, null);

            //assert
            postthread.Should().Be(queuethread);
        }

        [Fact]
        public async Task Send_Should_Run_Immediately_On_Queue_Thread()
        {
            //arrange
            var queueThread = await _Fiber.Enqueue(() => Thread.CurrentThread);

            //act
            Thread postThread = null;
            SendOrPostCallback post = (o) => { postThread = Thread.CurrentThread; };
            await _Fiber.Enqueue(() => _Dispatcher.Send(post, null));

            //assert
            postThread.Should().Be(queueThread);
        }

        [Fact]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext()
        {
            //arrange
            var cloned = _Dispatcher.CreateCopy();

            //assert
            cloned.Should().BeAssignableTo<MonoThreadedFiberSynchronizationContext>();
        }

        [Fact]
        public void CreateCopy_Should_Return_A_DispatcherSynchronizationContext_Conected_ToSameQueue()
        {
            //arrange
            var cloned = (MonoThreadedFiberSynchronizationContext)_Dispatcher.CreateCopy();

            //assert
            cloned.IsSame(_Dispatcher).Should().BeTrue();
        }
    }
}
