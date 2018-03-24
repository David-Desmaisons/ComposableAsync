using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Concurrent.Disposable
{
    public sealed class ComposableAsyncDisposable : IAsyncDisposable
    {
        private readonly ConcurrentQueue<IAsyncDisposable> _Disposables;
        public ComposableAsyncDisposable()
        {
            _Disposables = new ConcurrentQueue<IAsyncDisposable>();
        }

        public T Add<T>(T disposable) where T: IAsyncDisposable
        {
            if (disposable == null)
                return disposable;

            _Disposables.Enqueue(disposable);
            return disposable;
        }

        public ComposableAsyncDisposable(params IAsyncDisposable[] disposables)
        {
            _Disposables = new ConcurrentQueue<IAsyncDisposable>(disposables);
        }

        public Task DisposeAsync()
        {
            var tasks = _Disposables.ToArray().Select(disp => disp.DisposeAsync()).ToArray();
            return Task.WhenAll(tasks);
        }
    }
}
