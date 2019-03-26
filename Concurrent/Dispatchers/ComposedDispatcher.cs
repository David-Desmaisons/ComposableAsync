using System;
using System.Threading.Tasks;

namespace Concurrent.Dispatchers
{
    internal class ComposedDispatcher : IDispatcher
    {
        private readonly IDispatcher _First;
        private readonly IDispatcher _Second;

        public ComposedDispatcher(IDispatcher first, IDispatcher second)
        {
            _First = first;
            _Second = second;
        }

        public void Dispatch(Action action)
        {
            _First.Dispatch(() => _Second.Dispatch(action));
        }

        public async Task Enqueue(Action action)
        {
            await _First.Enqueue(() => _Second.Enqueue(action));
        }

        public async Task<T> Enqueue<T>(Func<T> action)
        {
            return await _First.Enqueue(() => _Second.Enqueue(action));
        }

        public async Task Enqueue(Func<Task> action)
        {
            await _First.Enqueue(() => _Second.Enqueue(action));
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return await _First.Enqueue(() => _Second.Enqueue(action));
        }
    }
}
