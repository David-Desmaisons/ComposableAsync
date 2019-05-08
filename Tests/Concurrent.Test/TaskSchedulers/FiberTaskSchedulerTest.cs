using System.Threading;
using System.Threading.Tasks;
using Concurrent.Fibers;
using Concurrent.TaskSchedulers;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.TaskSchedulers
{
    public class FiberTaskSchedulerTest : IAsyncLifetime
    {
        private readonly FiberTaskScheduler _FiberTaskScheduler;
        private readonly MonoThreadedFiber _MonoThreadedFiber;

        public FiberTaskSchedulerTest()
        {
            _MonoThreadedFiber = new MonoThreadedFiber();
            _FiberTaskScheduler = new FiberTaskScheduler(_MonoThreadedFiber);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => _MonoThreadedFiber.DisposeAsync();

        [Fact]
        public async Task Task_Run_On_Scheduler_Runs_On_Fiber_Thread()
        {
            var thread = await Task.Factory.StartNew(() => Thread.CurrentThread, CancellationToken.None, TaskCreationOptions.None, _FiberTaskScheduler);

            _MonoThreadedFiber.Thread.Should().Be(thread);
        }

        [Fact]
        public void MaximumConcurrencyLevel_Is_One()
        {
            _FiberTaskScheduler.MaximumConcurrencyLevel.Should().Be(1);
        }
    }
}
