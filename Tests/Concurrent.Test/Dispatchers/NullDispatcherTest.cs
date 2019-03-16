using System.Threading;
using System.Threading.Tasks;
using Concurrent.Dispatchers;
using Concurrent.Test.TestHelper;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Dispatchers
{
    public class NullDispatcherTest
    {
        private readonly IDispatcher _NullDispatcher;

        public NullDispatcherTest()
        {
            _NullDispatcher = NullDispatcher.Instance;
        }

        [Fact]
        public async Task Dispatch_Runs_On_Current_Thread()
        {
            var tcs = new TaskCompletionSource<Thread>();
            _NullDispatcher.Dispatch(() => tcs.TrySetResult(Thread.CurrentThread));
            var thread = await tcs.Task;
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Action_Runs_On_Current_Thread()
        {
            var thread = default(Thread);
            await _NullDispatcher.Enqueue(() => { thread = Thread.CurrentThread; });
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Func_T_Runs_On_UI_Thread()
        {
            var thread = await _NullDispatcher.Enqueue(() => Thread.CurrentThread);
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Runs_On_UI_Thread()
        {
            var thread = default(Thread);
            await _NullDispatcher.Enqueue(() =>
            {
                thread = Thread.CurrentThread;
                return Task.CompletedTask;
            });
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Func_Task_T_Runs_On_UI_Thread()
        {
            var thread = await _NullDispatcher.Enqueue(() => Task.FromResult(Thread.CurrentThread));
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact(Skip = "Machine dependant")]
        public async Task Enqueue_Action_Does_Not_Run_Actions_Sequencially()
        {
            var tester = new SequenceTester(_NullDispatcher, null);
            await tester.Stress();
            tester.Count.Should().BeLessThan(tester.MaxThreads);
        }

        [Fact(Skip = "Machine dependant")]
        public async Task Enqueue_Task_Does_Not_Run_Actions_Sequencially_after_await()
        {
            var tester = new SequenceTester(_NullDispatcher, null);
            await tester.StressTask();
            tester.Count.Should().BeLessThan(tester.MaxThreads);
        }
    }
}
