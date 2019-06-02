﻿using System.Threading.Tasks;
using ComposableAsync.Concurrent;

namespace ComposableAsync.Actors.DispatcherManagers
{
    internal sealed class TaskPoolFiberManager : IDispatcherManager
    {
        public bool DisposeDispatcher => true;

        public ICancellableDispatcher GetDispatcher() => Fiber.GetTaskBasedFiber();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
