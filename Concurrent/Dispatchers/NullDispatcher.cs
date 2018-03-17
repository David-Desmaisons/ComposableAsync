using System;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace Concurrent.Dispatchers
{
    internal class NullDispatcher: IDispatcher
    {
        public void Dispatch(Action action)
        {
            action();
        }

        public Task Enqueue(Action action)
        {
            action();
            return TaskBuilder.Completed;
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            return Task.FromResult(action());
        }

        public async Task Enqueue(Func<Task> action)
        {
            await action();
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return await action();
        }
    }
}
