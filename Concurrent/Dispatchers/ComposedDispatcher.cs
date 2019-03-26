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
            throw new NotImplementedException();
        }

        public Task Enqueue(Action action)
        {
            throw new NotImplementedException();
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            throw new NotImplementedException();
        }
    }
}
