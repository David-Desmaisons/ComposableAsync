using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Resilient.ExceptionFilter;
using ComposableAsync.Resilient.Retry.TimeOuts;

namespace ComposableAsync.Resilient.Retry
{
    internal sealed class RetryDispatcherAsync : IBasicDispatcher
    {
        private readonly int _MaxRetry;
        private readonly ITimeOutProvider _TimeOutProvider;
        private readonly IExceptionFilter _ExceptionFilter;

        internal RetryDispatcherAsync(IExceptionFilter exceptionFilter, ITimeOutProvider timeOutProvider, int maxRetry)
        {
            _MaxRetry = maxRetry;
            _TimeOutProvider = timeOutProvider;
            _ExceptionFilter = exceptionFilter;
        }

        public IBasicDispatcher Clone() => this;

        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await action();
                    return;
                }
                catch (Exception exception)
                {
                    if (ShouldRethrow(count, exception))
                        throw;

                    await WaitIfNeeded(count++, cancellationToken);
                }
            }
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return await action();
                }
                catch (Exception exception)
                {
                    if (ShouldRethrow(count, exception))
                        throw;

                    await WaitIfNeeded(count++, cancellationToken);
                }
            }
        }

        public async Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return action();
                }
                catch (Exception exception)
                {
                    if (ShouldRethrow(count, exception))
                        throw;

                    await WaitIfNeeded(count++, cancellationToken);
                }
            }
        }

        public async Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    action();
                    return;
                }
                catch (Exception exception)
                {
                    if (ShouldRethrow(count, exception))
                        throw;

                    await WaitIfNeeded(count++, cancellationToken);
                }
            }
        }

        private bool ShouldRethrow(int count, Exception exception)
        {
            return (count == _MaxRetry) || (_ExceptionFilter.IsFiltered(exception));
        }

        private async Task WaitIfNeeded(int count, CancellationToken cancellationToken)
        {
            var wait = _TimeOutProvider.GetTimeOutForRetry(count);
            if (Math.Abs(wait.TotalMilliseconds) <= 0)
                return;
            await Task.Delay(wait, cancellationToken);
        }
    }
} 
