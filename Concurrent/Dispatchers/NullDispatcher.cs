﻿using System;
using System.Threading.Tasks;

namespace Concurrent.Dispatchers
{
    /// <summary>
    /// <see cref="IDispatcher"/> that run actions synchronously
    /// </summary>
    public sealed class NullDispatcher: IDispatcher
    {
        private NullDispatcher() { }

        /// <summary>
        /// Returns a static null dispatcher
        /// </summary>
        public static IDispatcher Instance { get; } = new NullDispatcher();

        public void Dispatch(Action action)
        {
            action();
        }

        public Task Enqueue(Action action)
        {
            action();
            return Task.CompletedTask;
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
