using System.Threading;
using System.Threading.Tasks;
using Concurrent.Fibers;
using Concurrent.SynchronizationContexts;
using Concurrent.Tasks;
using Concurrent.Test.TestHelper;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Fibers
{
    public class TaskSchedulerFiberTest
    {
        private readonly TaskSchedulerFiber _TaskSchedulerFiber;

        public TaskSchedulerFiberTest()
        {
            _TaskSchedulerFiber = new TaskSchedulerFiber();
        }

        [Fact]
        public void SynchronizationContext_Returns_A_TaskSchedulerSynchronizationContext()
        {
            var actual = _TaskSchedulerFiber.SynchronizationContext;
            actual.Should().BeAssignableTo<TaskSchedulerSynchronizationContext>();
        }

        [Fact]
        public void SynchronizationContext_Has_Correct_TaskScheduler()
        {
            var expected = _TaskSchedulerFiber.TaskScheduler;

            var context = (TaskSchedulerSynchronizationContext)_TaskSchedulerFiber.SynchronizationContext;
            var actual = context.TaskFactory.Scheduler;

            actual.Should().Be(expected);
        }

        [Fact]
        public void IsAlive_Returns_True()
        {
            var actual = _TaskSchedulerFiber.IsAlive;
            actual.Should().BeTrue();
        }

        [Fact]
        public async Task Dispatch_Runs_On_ThreadPool_Thead()
        {
            var tcs = new TaskCompletionSource<Thread>();
            _TaskSchedulerFiber.Dispatch(() => tcs.TrySetResult(Thread.CurrentThread));
            var thread = await tcs.Task;
            thread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Enqueue_Action_Runs_On_ThreadPool_Thead()
        {
            var thread = default(Thread);
            await _TaskSchedulerFiber.Enqueue(() => { thread = Thread.CurrentThread; });
            thread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Enqueue_Func_T_Runs_On_ThreadPool_Thead()
        {
            var thread = await _TaskSchedulerFiber.Enqueue(() => Thread.CurrentThread);
            thread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Enqueue_Func_Task_Runs_On_ThreadPool_Thead()
        {
            var thread = default(Thread);
            await _TaskSchedulerFiber.Enqueue(() =>
            {
                thread = Thread.CurrentThread;
                return TaskBuilder.Completed;
            });
            thread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Enqueue_Func_Task_T_Continue_After_Await_On_Scheduler()
        {
            var scheduler = await _TaskSchedulerFiber.Enqueue(async () =>
            {
                await Task.Yield();
                return TaskScheduler.Current;
            });
            scheduler.Should().Be(_TaskSchedulerFiber.TaskScheduler);
        }

        [Fact]
        public async Task Enqueue_Func_Task_T_Runs_On_ThreadPool_Thead()
        {
            var thread = await _TaskSchedulerFiber.Enqueue(() => Task.FromResult(Thread.CurrentThread));
            thread.IsThreadPoolThread.Should().BeTrue();
        }

        [Fact]
        public async Task Enqueue_Action_Runs_Actions_Sequencially()
        {
            var tester = new SequenceTester(_TaskSchedulerFiber);
            await tester.Stress();
            tester.Count.Should().Be(tester.MaxThreads);
        }

        [Fact]
        public async Task Enqueue_Task_Runs_Actions_Sequencially_after_await()
        {
            var tester = new SequenceTester(_TaskSchedulerFiber);
            await tester.StressTask();
            tester.Count.Should().Be(tester.MaxThreads);
        }
    }
}
