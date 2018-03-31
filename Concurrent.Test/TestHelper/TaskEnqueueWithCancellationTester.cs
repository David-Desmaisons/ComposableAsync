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
            var newTask = _CancellableDispatcher.Enqueue(() => TaskFactory_T(() => CancelledTaskHasBeenExcecuted = true), cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);

            _Stopwatch.Stop();

            await taskEnqueued;

            await Task.Delay(200);
        }


        private Task<int> TaskFactory_T(Action @do = null, int sleep = 0)
        {
            @do?.Invoke();
            Thread.Sleep(sleep * 1000);
            return Task.FromResult(0);
        }

        private Task TaskFactory(Action @do = null, int sleep = 0)
        {
            @do?.Invoke();
            Thread.Sleep(sleep * 1000);
            return TaskBuilder.Completed;
        }

        private static async Task AwaitForCancellation(Task toBeCancelled)
        {
            var cancelled = false;
            try
            {
                await toBeCancelled;
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            cancelled.Should().BeTrue();
        }
    }
}
