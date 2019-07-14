using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal class ForEverRetryDispatcher : IBasicDispatcher
    {
        public IBasicDispatcher Clone()
        {
            return this;
        }

        public void Dispatch(Action action)
        {
            Enqueue(action, CancellationToken.None);
        }

        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await action();
                    return;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return await action();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
