using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal class ForEverRetrySelectiveDispatcher : IBasicDispatcher
    {
        private readonly HashSet<Type> _Types;

        internal ForEverRetrySelectiveDispatcher(HashSet<Type> types)
        {
            _Types = types;
        }

        public IBasicDispatcher Clone()
        {
            return this;
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
                catch (Exception exception)
                {
                    ThrowIfNeeded(exception);
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
                catch (Exception exception)
                {
                    ThrowIfNeeded(exception);
                }
            }
        }

        private void ThrowIfNeeded(Exception exception)
        {
            var type = exception.GetType();
            if (_Types.Any(t => t == type || type.IsSubclassOf(t)))
                return;

            throw exception;
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
                catch (Exception exception)
                {
                    ThrowIfNeeded(exception);
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
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
