﻿using System;
using System.Threading.Tasks;

namespace ComposableAsync
{
    public sealed class MonoDispatcherManager : IDispatcherManager
    {
        public bool DisposeDispatcher { get; }
        public ICancellableDispatcher GetDispatcher() => _Dispatcher;
        public Task DisposeAsync()
        {
            return DisposeDispatcher && (_Dispatcher is IAsyncDisposable disposable) ?
                disposable.DisposeAsync() : Task.CompletedTask;
        }

        private readonly ICancellableDispatcher _Dispatcher;

        public MonoDispatcherManager(ICancellableDispatcher dispatcher, bool shouldDispose = false)
        {
            _Dispatcher = dispatcher;
            DisposeDispatcher = shouldDispose;
        }
    }
}
