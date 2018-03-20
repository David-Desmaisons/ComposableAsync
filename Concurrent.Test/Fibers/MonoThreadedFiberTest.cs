using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Fibers;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Fibers
{
    public class MonoThreadFiberTest : MonoThreadedFiberBaseTest
    {
        protected override IMonoThreadFiber GetFiber(Action<Thread> onCreate = null) => new MonoThreadedFiber(onCreate);

        [Fact]
        public async Task Enqueue_Should_Run_On_Dedicated_Thread()
        {
            var target = GetSafeFiber();
                await target.Enqueue(() => TaskFactory());

                RunningThread.IsThreadPoolThread.Should().BeFalse();
        }
    }
}
