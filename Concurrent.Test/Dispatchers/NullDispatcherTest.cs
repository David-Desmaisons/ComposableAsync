using System.Threading;
using System.Threading.Tasks;
using Concurrent.Dispatchers;
using Concurrent.Tasks;
using Concurrent.Test.TestHelper;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Dispatchers
{
    public class NullDispatcherTest
    {
        private readonly IDispatcher _NullDispatcherTest;

        public NullDispatcherTest()
        {
            _NullDispatcherTest = NullDispatcher.Instance;
        }

        [Fact]
        public async Task Dispatch_Runs_On_Current_Thread()
        {
            var tcs = new TaskCompletionSource<Thread>();
            _NullDispatcherTest.Dispatch(() => tcs.TrySetResult(Thread.CurrentThread));
            var thread = await tcs.Task;
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Action_Runs_On_Current_Thread()
        {
            var thread = default(Thread);
            await _NullDispatcherTest.Enqueue(() => { thread = Thread.CurrentThread; });
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Func_T_Runs_On_UI_Thread()
        {
            var thread = await _NullDispatcherTest.Enqueue(() => Thread.CurrentThread);
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Runs_On_UI_Thread()
        {
            var thread = default(Thread);
            await _NullDispatcherTest.Enqueue(() =>
            {
                thread = Thread.CurrentThread;
                return TaskBuilder.Completed;
            });
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact]
        public async Task Enqueue_Func_Task_T_Runs_On_UI_Thread()
        {
            var thread = await _NullDispatcherTest.Enqueue(() => Task.FromResult(Thread.CurrentThread));
            thread.Should().Be(Thread.CurrentThread);
        }

        [Fact(Skip = "Machine dependant")]
        public async Task Enqueue_Action_Does_Not_Run_Actions_Sequencially()
        {
            var tester = new SequenceTester(_NullDispatcherTest);
            await tester.Stress();
            tester.Count.Should().BeLessThan(tester.MaxThreads);
        }

        [Fact(Skip = "Machine dependant")]
        public async Task Enqueue_Task_Does_Not_Run_Actions_Sequencially_after_await()
        {
            var tester = new SequenceTester(_NullDispatcherTest);
            await tester.StressTask();
            tester.Count.Should().BeLessThan(tester.MaxThreads);
        }
    }
}
