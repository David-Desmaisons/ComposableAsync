using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Dispatchers;
using Concurrent.Fibers;
using Concurrent.Tasks;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test
{
    public class FiberTest : IAsyncLifetime
    {
        private IStopableFiber _Fiber;

        public Task InitializeAsync() => TaskBuilder.Completed;

        public Task DisposeAsync()
        {
            return _Fiber?.DisposeAsync() ?? TaskBuilder.Completed;
        }

        [Fact]
        public void CreateMonoThreadedFiber_Returns_MonoThreadedFiber()
        {
            _Fiber = Fiber.CreateMonoThreadedFiber();
            _Fiber.Should().BeAssignableTo<MonoThreadedFiber>();
        }

        [Fact]
        public void GetThreadPoolFiber_Returns_ThreadPoolFiber()
        {
            _Fiber = Fiber.GetThreadPoolFiber();
            _Fiber.Should().BeAssignableTo<ThreadPoolFiber>();
        }

        [Fact]
        public void GetTaskBasedFiber_Returns_TaskSchedulerFiber()
        {
            _Fiber = Fiber.GetTaskBasedFiber();
            _Fiber.Should().BeAssignableTo<TaskSchedulerFiber>();
        }

        [Fact]
        public void GetFiberFromSynchronizationContext_Returns_SynchronizationContextFiber_With_Correct_Context()
        {
            var context = new SynchronizationContext();
            var fiber = Fiber.GetFiberFromSynchronizationContext(context);
            fiber.Should().BeAssignableTo<SynchronizationContextFiber>();
            ((SynchronizationContextFiber)fiber).SynchronizationContext.Should().Be(context);
        }

        public static IEnumerable<object[]> SynchronizationContexts
        {
            get
            {
                yield return new object[] { null };
                yield return new object[] { new SynchronizationContext() };
            }
        }

        [Theory]
        [MemberData(nameof(SynchronizationContexts))]
        public void GetFiberFromCurrentContext_Returns_Fiber_Only_When_Context_Is_NotNull(SynchronizationContext synchronizationContext)
        {
            var ctx = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            var fiber = Fiber.GetFiberFromCurrentContext();
            SynchronizationContext.SetSynchronizationContext(ctx);

            if (synchronizationContext == null)
            {
                fiber.Should().BeNull();
                return;
            }

            fiber.Should().BeAssignableTo<SynchronizationContextFiber>();
            ((SynchronizationContextFiber)fiber).SynchronizationContext.Should().Be(synchronizationContext);
        }

        [Theory]
        [MemberData(nameof(SynchronizationContexts))]
        public void GetDispatcherFromCurrentContext_Returns_Fiber_Only_When_Context_Is_NotNull(SynchronizationContext synchronizationContext)
        {
            var ctx = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            var fiber = Fiber.GetDispatcherFromCurrentContext();
            SynchronizationContext.SetSynchronizationContext(ctx);

            if (synchronizationContext == null)
            {
                fiber.Should().BeAssignableTo<NullDispatcher>();
                return;
            }

            fiber.Should().BeAssignableTo<SynchronizationContextFiber>();
            ((SynchronizationContextFiber)fiber).SynchronizationContext.Should().Be(synchronizationContext);
        }
    }
}
