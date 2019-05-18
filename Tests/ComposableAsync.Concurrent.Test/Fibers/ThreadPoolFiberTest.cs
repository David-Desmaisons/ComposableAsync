using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.Collections;
using ComposableAsync.Concurrent.Fibers;
using ComposableAsync.Concurrent.WorkItems;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ComposableAsync.Concurrent.Test.Fibers
{
    public class ThreadPoolFiberTest : MonoThreadedFiberBaseTest
    {
        public ThreadPoolFiberTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override IMonoThreadFiber GetFiber(Action<Thread> onCreate = null) => new ThreadPoolFiber();
        protected override IMonoThreadFiber GetFiber(IMpScQueue<IWorkItem> queue) => new ThreadPoolFiber(queue);

        [Fact]
        public async Task Enqueue_Should_Run_On_PoolThread_Thread()
        {
            var target = GetSafeFiber();
            await target.Enqueue(() => TaskFactory());

            RunningThread.IsThreadPoolThread.Should().BeTrue();
        }
    }
}
