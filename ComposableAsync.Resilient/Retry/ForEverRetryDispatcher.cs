using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Retry
{
    internal class ForEverRetryDispatcher : IBasicDispatcher
    {
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
                catch (Exception)
                {
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
                catch (Exception)
                {
                }
            }
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
