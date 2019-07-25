using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.Retry
{
    internal sealed class RetryDispatcherAsync : IBasicDispatcher
    {
        private readonly int _MaxRetry;
        private readonly TimeSpan[] _TimeSpans;
        private readonly IExceptionFilter _ExceptionFilter;

        internal RetryDispatcherAsync(IExceptionFilter exceptionFilter, int maxRetry, TimeSpan[] timeSpans)
        {
            _MaxRetry = maxRetry;
            _TimeSpans = timeSpans;
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
            var wait = GetWait(count);
            if (Math.Abs(wait.TotalMilliseconds) <= 0) {
                return;
            }
            await Task.Delay(wait, cancellationToken);
        }

        private TimeSpan GetWait(int count)
        {
            var length = _TimeSpans.Length - 1;
            return(count > length) ? _TimeSpans[length] : _TimeSpans[count];
        }
    }
} 
