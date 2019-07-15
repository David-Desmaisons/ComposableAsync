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
        private readonly int _Max;

        internal ForEverRetrySelectiveDispatcher(HashSet<Type> types, int max = Int32.MaxValue)
        {
            _Types = types;
            _Max = max;
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

        private void ThrowIfNeeded(ref int count, Exception exception)
        {
            if (count++ == _Max)
                throw exception;

            var type = exception.GetType();
            if (_Types.Any(t => t == type || type.IsSubclassOf(t)))
                return;

            throw exception;
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
    }
}
