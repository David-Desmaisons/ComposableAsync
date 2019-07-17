using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal class ForEverRetryDispatcher : IBasicDispatcher
    {
        private readonly int _MaxRetry;

        public ForEverRetryDispatcher(int maxRetry)
        {
            _MaxRetry = maxRetry;
        }

        public IBasicDispatcher Clone()
        {
            return this;
        }

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
            //var count = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return await action();
                }
                catch (Exception exception)
                {
                    //ThrowIfNeeded(ref count, exception);
                }
            }
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return Task.FromResult(action());
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
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
                    //ThrowIfNeeded(ref int count, Exception exception);
                }
            }
        }

        private void ThrowIfNeeded(ref int count, Exception exception)
        {
            if (count++ == _MaxRetry)
                throw exception;
        }
    }
}
