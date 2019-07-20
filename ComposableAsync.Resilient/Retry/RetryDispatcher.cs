using System;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Resilient.Retry.ExceptionFilter;

namespace ComposableAsync.Resilient.Retry
{
    internal sealed class RetryDispatcher : IBasicDispatcher
    {
        private readonly int _MaxRetry;
        private readonly IExceptionFilter _ExceptionFilter;

        internal RetryDispatcher(IExceptionFilter exceptionFilter, int maxRetry)
        {
            _ExceptionFilter = exceptionFilter;
            _MaxRetry = maxRetry;
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
            if ((count++ != _MaxRetry) && (!_ExceptionFilter.ShouldBeThrown(exception)))
                return;
            throw exception;
        }
    }
}
