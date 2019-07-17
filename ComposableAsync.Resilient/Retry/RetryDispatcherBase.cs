using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal abstract class RetryDispatcherBase
    {
        private readonly int _MaxRetry;

        public RetryDispatcherBase(int maxRetry)
        {
            _MaxRetry = maxRetry;
        }

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
                    ThrowIfNeeded(ref count, exception);
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
                    ThrowIfNeeded(ref count, exception);
                }
            }
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return Task.FromResult(action());
                }
                catch (Exception exception)
                {
                    ThrowIfNeeded(ref count, exception);
                }
            }
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    action();
                    return Task.CompletedTask;
                }
                catch (Exception exception)
                {
                    ThrowIfNeeded(ref count, exception);
                }
            }
        }

        private void ThrowIfNeeded(ref int count, Exception exception)
        {
            if (count++ == _MaxRetry)
                throw exception;

            RethrowIfNeeded(exception);
        }
    }
}
