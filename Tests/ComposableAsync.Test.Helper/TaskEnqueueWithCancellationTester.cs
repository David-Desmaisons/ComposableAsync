using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace ComposableAsync.Test.Helper
{
    public class TaskEnqueueWithCancellationTester
    {
        private readonly IDispatcher _Dispatcher;
        private Stopwatch _Stopwatch;

        public TimeSpan TimeSpentToCancelTask => _Stopwatch.Elapsed;
        public bool CancelledTaskHasBeenExecuted { get; private set; } = false;

        public TaskEnqueueWithCancellationTester(IDispatcher cancellableDispatcher)
        {
            _Dispatcher = cancellableDispatcher;
        }

        public async Task RunAndCancelTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _Stopwatch = Stopwatch.StartNew();

            var taskEnqueued = _Dispatcher.Enqueue(() => TaskFactory(sleep: 2));
            var newTask = _Dispatcher.Enqueue(() => TaskFactory(() => CancelledTaskHasBeenExecuted = true), cancellationTokenSource.Token);

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

            var taskEnqueued = _Dispatcher.Enqueue(() => TaskFactory_T(sleep: 2));
            var newTask = _Dispatcher.Enqueue(() => TaskFactory_T(0, () => CancelledTaskHasBeenExecuted = true), cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);

            _Stopwatch.Stop();

            await taskEnqueued;

            await Task.Delay(200);
        }

        public async Task CancelCancellableRunningTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _Dispatcher.Enqueue(() => TaskFactory(cancellationTokenSource.Token, () => CancelledTaskHasBeenExecuted = true, sleep: 1),
                                    cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await taskEnqueued;
        }

        public async Task<int> CancelCancellableRunningTask_T()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _Dispatcher.Enqueue(() => TaskFactory_T(cancellationTokenSource.Token, () => CancelledTaskHasBeenExecuted = true, sleep: 1),
                cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            return await taskEnqueued;
        }

        public async Task CancelNotCancellableRunningTask()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _Dispatcher.Enqueue(() => TaskFactory(() => CancelledTaskHasBeenExecuted = true, sleep: 1), cancellationTokenSource.Token);

            await Task.Delay(200);
            cancellationTokenSource.Cancel();

            await taskEnqueued;
        }

        public async Task<int> CancelNotCancellableRunningTask_T(int value)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var taskEnqueued = _Dispatcher.Enqueue(() => TaskFactory_T(value: value, sleep: 1), cancellationTokenSource.Token);

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
            return Task.CompletedTask;
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
            return Task.CompletedTask;
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

        public Task RunAndThrow<T>() where T: Exception
        {
            return RunAndThrow(CancellationToken.None, Activator.CreateInstance<T>());
        }

        public Task RunAndThrow(CancellationToken token, Exception exception, Action before=null)
        {
            async Task Explode()
            {
                before?.Invoke();
                throw exception;
            }
            return _Dispatcher.Enqueue(Explode, token);
        }
    }
}
