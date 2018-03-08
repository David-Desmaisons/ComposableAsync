using System;
using System.Threading.Tasks;

namespace EasyActor.Fiber
{
    internal class AsyncWorkItem<T> : IWorkItem
    {
        private readonly TaskCompletionSource<T> _Source;
        private readonly Func<Task<T>> _Do;

        public AsyncWorkItem(Func<Task<T>> @do)
        {
            _Do = @do;
            _Source = new TaskCompletionSource<T>();
        }

        public Task<T> Task => _Source.Task;

        public void Cancel()
        {
            _Source.TrySetCanceled();
        }

        public async void Do()
        {
            try
            {
                _Source.TrySetResult(await _Do());
            }
            catch(Exception e)
            {
                _Source.TrySetException(e);
            }
        }
    }

    internal class AsyncActionWorkItem : AsyncWorkItem<object>, IWorkItem 
    {
        public AsyncActionWorkItem(Func<Task> @do)
            : base(async () => { await @do(); return null; })
        {
        }
    }
}
