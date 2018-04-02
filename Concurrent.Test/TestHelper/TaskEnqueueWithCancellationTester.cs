using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;
using FluentAssertions;

namespace Concurrent.Test.TestHelper
{
    public class TaskEnqueueWithCancellationTester
    {
        private readonly ICancellableDispatcher _CancellableDispatcher;
        private Stopwatch _Stopwatch;

        public TimeSpan TimeSpentToCancellTask => _Stopwatch.Elapsed;
        public bool CancelledTaskHasBeenExcecuted { get; private set; } = false;

        public TaskEnqueueWithCancellationTester(ICancellableDispatcher cancellableDispatcher)
        {
            _CancellableDispatcher = cancellableDispatcher;
        }

        public async Task RunAndCancelTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _Stopwatch = Stopwatch.StartNew();

            var taskEnqueued = _CancellableDispatcher.Enqueue(() => TaskFactory(sleep: 2));
            var newTask = _CancellableDispatcher.Enqueue(() => TaskFactory(() => CancelledTaskHasBeenExcecuted = true), cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);

            _Stopwatch.Stop();

            await taskEnqueued;

            await Task.Delay(200);
        }

        public async Task RunAndCancelTask_T()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _Stopwatch = Stopwatch.StartNew();

            var taskEnqueued = _CancellableDispatcher.Enqueue(() => TaskFactory_T(sleep: 2));
            var newTask = _CancellableDispatcher.Enqueue(() => TaskFactory_T(0, () => CancelledTaskHasBeenExcecuted = true), cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);

            _Stopwatch.Stop();

            await taskEnqueued;

            await Task.Delay(200);
        }

        public async Task CancelCancellableRunningTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _CancellableDispatcher.Enqueue(() => TaskFactory(cancellationTokenSource.Token, () => CancelledTaskHasBeenExcecuted = true, sleep: 1),
                                    cancellationTokenSource.Token);

            await Task.Delay(200);
            cancellationTokenSource.Cancel();

            await taskEnqueued;
        }

        public async Task<int> CancelCancellableRunningTask_T()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _CancellableDispatcher.Enqueue(() => TaskFactory_T(cancellationTokenSource.Token, () => CancelledTaskHasBeenExcecuted = true, sleep: 1),
                cancellationTokenSource.Token);

            await Task.Delay(200);
            cancellationTokenSource.Cancel();

            return await taskEnqueued;
        }

        public async Task CancelNotCancellableRunningTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _CancellableDispatcher.Enqueue(() => TaskFactory(() => CancelledTaskHasBeenExcecuted = true, sleep: 1), cancellationTokenSource.Token);

            await Task.Delay(200);
            cancellationTokenSource.Cancel();

            await taskEnqueued;
        }

        public async Task<int> CancelNotCancellableRunningTask_T(int value)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _CancellableDispatcher.Enqueue(() => TaskFactory_T(value: value, sleep: 1), cancellationTokenSource.Token);

            await Task.Delay(200);
            cancellationTokenSource.Cancel();

            return await taskEnqueued;
        }

        private Task<int> TaskFactory_T(int value = 0, Action @do = null, int sleep = 0)
        {
            @do?.Invoke();
            Thread.Sleep(sleep * 1000);
            return Task.FromResult(value);
        }

        private Task TaskFactory(Action @do = null, int sleep = 0)
        {
            @do?.Invoke();
            Thread.Sleep(sleep * 1000);
            return TaskBuilder.Completed;
        }

        private Task<int> TaskFactory_T(CancellationToken cancellationToken, Action @do = null, int sleep = 0)
        {
            @do?.Invoke();
            Thread.Sleep(sleep * 1000);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(0);
        }

        private Task TaskFactory(CancellationToken cancellationToken, Action @do = null, int sleep = 0)
        {
            @do?.Invoke();
            Thread.Sleep(sleep * 1000);
            cancellationToken.ThrowIfCancellationRequested();
            return TaskBuilder.Completed;
        }

        public static async Task<Exception> AwaitForException(Task toBeCancelled)
        {
            try
            {
                await toBeCancelled;
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static async Task AwaitForCancellation(Task toBeCancelled)
        {

            var exception = await AwaitForException(toBeCancelled);
            exception.Should().BeAssignableTo<TaskCanceledException>();
        }
    }
}
