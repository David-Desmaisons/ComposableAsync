using System;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter;

namespace ComposableAsync.Concurrent.Dispatchers
{
    /// <summary>
    /// Dispatcher using rate limiting
    /// </summary>
    public sealed class RateLimiterDispatcher: ICancellableDispatcher
    {
        private readonly IRateLimiter _RateLimiter;

        /// <summary>
        /// Construct an RateLimiterDispatcher from an <see cref="IAwaitableConstraint"/>
        /// </summary>
        /// <param name="rateLimiter"></param>
        public RateLimiterDispatcher(IRateLimiter rateLimiter)
        {
            _RateLimiter = rateLimiter;
        }

        /// <summary>
        /// Dispatch the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        public void Dispatch(Action action)
        {
            _RateLimiter.Perform(action);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task Enqueue(Func<Task> action)
        {
            return _RateLimiter.Perform(action);
        }

        /// <summary>
        /// Enqueue the function respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return _RateLimiter.Perform(action);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task Enqueue(Action action)
        {
            return _RateLimiter.Perform(action);
        }

        /// <summary>
        /// Enqueue the function respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task<T> Enqueue<T>(Func<T> action)
        {
            return _RateLimiter.Perform(action);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            return _RateLimiter.Perform(action, cancellationToken);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            return _RateLimiter.Perform(action, cancellationToken);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            return _RateLimiter.Perform(action, cancellationToken);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            return _RateLimiter.Perform(action, cancellationToken);
        }
    }
}
