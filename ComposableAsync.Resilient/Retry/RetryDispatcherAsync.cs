using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Retry.ExceptionFilter;

namespace ComposableAsync.Retry
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
                    count = await ThrowIfNeeded(count, exception, cancellationToken);
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
                    count = await ThrowIfNeeded(count, exception, cancellationToken);
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
                    count = await ThrowIfNeeded(count, exception, cancellationToken);
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
                    count = await ThrowIfNeeded(count, exception, cancellationToken);
                }
            }
        }

        private async Task<int> ThrowIfNeeded(int count, Exception exception, CancellationToken cancellationToken)
        {
            if ((count == _MaxRetry) || (_ExceptionFilter.ShouldBeThrown(exception)))
                throw exception;

            var wait = GetWait(count);
            if (Math.Abs(wait.TotalMilliseconds) > 0) {
                await Task.Delay(wait, cancellationToken);
            }
            
            return count + 1;
        }

        private TimeSpan GetWait(int count)
        {
            var length = _TimeSpans.Length - 1;
            return(count > length) ? _TimeSpans[length] : _TimeSpans[count];
        }
    }
} 
