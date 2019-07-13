using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal class ForEverRetrySelectiveDispatcher : IBasicDispatcher
    {
        private readonly HashSet<Type> _Types;

        public ForEverRetrySelectiveDispatcher(HashSet<Type> types)
        {
            _Types = types;
        }

        public IBasicDispatcher Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispatch(Action action)
        {
            throw new NotImplementedException();
        }

        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            var done = false;
            do
            {
                try
                {
                    await action();
                    done = true;
                }
                catch (Exception exception)
                {
                    ThrowIfNeeded(exception);
                }
            } while (done == false);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            while (true)
            {
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
            if (_Types.Contains(exception.GetType()))
                return;

            throw exception;
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
