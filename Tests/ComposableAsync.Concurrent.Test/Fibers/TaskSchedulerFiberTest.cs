using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent.Fibers;
using ComposableAsync.Concurrent.SynchronizationContexts;
using ComposableAsync.Test.Helper;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ComposableAsync.Concurrent.Test.Fibers
{
    public class TaskSchedulerFiberTest : IAsyncLifetime
    {
        private readonly TaskSchedulerFiber _TaskSchedulerFiber;
        private readonly ITestOutputHelper _TestOutputHelper;

        public TaskSchedulerFiberTest(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
            _TaskSchedulerFiber = new TaskSchedulerFiber();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync()
        {
            return _TaskSchedulerFiber.DisposeAsync();
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
                return Task.CompletedTask;
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
            var tester = new SequenceTester(_TaskSchedulerFiber, _TestOutputHelper);
            await tester.Stress();
            tester.Count.Should().Be(tester.MaxThreads);
        }

        [Fact]
        public async Task Enqueue_Task_Runs_Actions_Sequencially_after_await()
        {
            var tester = new SequenceTester(_TaskSchedulerFiber, _TestOutputHelper);
            await tester.StressTask();
            tester.Count.Should().Be(tester.MaxThreads);
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Rethrows_Exception()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);

            Func<Task> @do = () => tester.RunAndThrow<ArgumentException>();

            await @do.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Rethrows_OperationCanceledException()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);

            Func<Task> @do = () => tester.RunAndThrow<OperationCanceledException>();

            await @do.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Rethrows_OperationCanceledException_Corresponding_To_Same_Token()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);
            var cancellationTokenSource = new CancellationTokenSource();

            Func<Task> @do = () => tester.RunAndThrow(cancellationTokenSource.Token, new OperationCanceledException(cancellationTokenSource.Token),
                () => cancellationTokenSource.Cancel());

            await @do.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Immediately_Cancel_Tasks_Enqueued()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);

            await tester.RunAndCancelTask_T();

            tester.TimeSpentToCancelTask.Should().BeLessThan(TimeSpan.FromSeconds(0.5));
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Do_Not_Run_Task_Cancelled_Enqueued()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);

            await tester.RunAndCancelTask_T();

            tester.CancelledTaskHasBeenExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_Task_With_Cancellation_Immediately_Cancel_Tasks_Enqueued()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);

            await tester.RunAndCancelTask();

            tester.TimeSpentToCancelTask.Should().BeLessThan(TimeSpan.FromSeconds(0.5));
        }

        [Fact]
        public async Task Enqueue_Task_With_Cancellation_Do_Not_Run_Task_Cancelled_Enqueued()
        {
            var tester = new TaskEnqueueWithCancellationTester(_TaskSchedulerFiber);

            await tester.RunAndCancelTask();

            tester.CancelledTaskHasBeenExecuted.Should().BeFalse();
        }
    }
}
