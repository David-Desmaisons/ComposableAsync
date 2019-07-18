using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal abstract class RetryDispatcherAsyncBase : IBasicDispatcher
    {
        private readonly int _MaxRetry;
        private readonly TimeSpan[] _TimeSpans;

        protected RetryDispatcherAsyncBase(int maxRetry, TimeSpan[] timeSpans)
        {
            _MaxRetry = maxRetry;
            _TimeSpans = timeSpans;
        }

        public IBasicDispatcher Clone() => this;

        protected abstract void RethrowIfNeeded(Exception exception);

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
            if (count == _MaxRetry)
                throw exception;

            RethrowIfNeeded(exception);

            var wait = GetWait(count);
            if (Math.Abs(wait.TotalMilliseconds) > 0) {
                await Task.Delay(wait, cancellationToken);
            }
            
            return count + 1;
        }

        private TimeSpan GetWait(int count)
        {
            var length = _TimeSpans.Length - 1;
            if (count > length)
                return _TimeSpans[length];
            return _TimeSpans[count];
        }
    }
}
