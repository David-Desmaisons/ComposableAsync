using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test
{
    public class FiberExtensionTest : IAsyncLifetime
    {
        private readonly IMonoThreadFiber _Fiber;
        private Thread _FiberThread;

        public FiberExtensionTest()
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
    }
}
