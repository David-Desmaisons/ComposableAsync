using System;
using System.Threading.Tasks;

namespace ComposableAsync
{
    /// <summary>
    /// <see cref="IDispatcherManager"/> implementation based on single <see cref="ICancellableDispatcher"/>
    /// </summary>
    public sealed class MonoDispatcherManager : IDispatcherManager
    {
        /// <inheritdoc cref="IDispatcherManager"/>
        public bool DisposeDispatcher { get; }

        /// <inheritdoc cref="IDispatcherManager"/>
        public ICancellableDispatcher GetDispatcher() => _Dispatcher;

        private readonly ICancellableDispatcher _Dispatcher;

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="shouldDispose"></param>
        public MonoDispatcherManager(ICancellableDispatcher dispatcher, bool shouldDispose = false)
        {
            _Dispatcher = dispatcher;
            DisposeDispatcher = shouldDispose;
        }

        /// <inheritdoc cref="IDispatcherManager"/>
        public Task DisposeAsync()
        {
            return DisposeDispatcher && (_Dispatcher is IAsyncDisposable disposable) ?
                disposable.DisposeAsync() : Task.CompletedTask;
        }
    }
}
