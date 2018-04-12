using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Fibers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Concurrent.Test.Fibers
{
    public class ThreadPoolFiberTest : MonoThreadedFiberBaseTest
    {
        public ThreadPoolFiberTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override IMonoThreadFiber GetFiber(Action<Thread> onCreate = null) => new ThreadPoolFiber();

        [Fact]
        public async Task Enqueue_Should_Run_On_PoolThread_Thread()
        {
            var target = GetSafeFiber();
            await target.Enqueue(() => TaskFactory());

            RunningThread.IsThreadPoolThread.Should().BeTrue();
        }
    }
}
