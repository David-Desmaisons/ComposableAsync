using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Concurrent.Disposable
{
    public class ComposableAsyncDisposable : IAsyncDisposable
    {
        private readonly ConcurrentQueue<IAsyncDisposable> _Disposables;
        public ComposableAsyncDisposable()
        {
            _Disposables = new ConcurrentQueue<IAsyncDisposable>();
        }

        public void Add(IAsyncDisposable disposable)
        {
            if (disposable == null)
                return;

            _Disposables.Enqueue(disposable);
        }

        public ComposableAsyncDisposable(params IAsyncDisposable[] disposables)
        {
            _Disposables = new ConcurrentQueue<IAsyncDisposable>(disposables);
        }

        public void Dispose() => DisposeAsync().Wait();

        public async Task DisposeAsync()
        {
            IAsyncDisposable actordisp;
            while (_Disposables.TryDequeue(out actordisp))
            {
                await actordisp.DisposeAsync();
            }
        }
    }
}
