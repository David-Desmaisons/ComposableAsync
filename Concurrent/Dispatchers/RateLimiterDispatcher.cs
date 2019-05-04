using System;
using System.Threading;
using System.Threading.Tasks;
using RateLimiter;

namespace Concurrent.Dispatchers
{
    /// <summary>
    /// Dispatcher using rate limiting
    /// </summary>
    public sealed class RateLimiterDispatcher: ICancellableDispatcher
    {
        private readonly IAwaitableConstraint _AwaitableConstraint;

        /// <summary>
        /// Construct an RateLimiterDispatcher from an <see cref="IAwaitableConstraint"/>
        /// </summary>
        /// <param name="awaitableConstraint"></param>
        public RateLimiterDispatcher(IAwaitableConstraint awaitableConstraint)
        {
            _AwaitableConstraint = awaitableConstraint;
        }

        /// <summary>
        /// Dispatch the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        public void Dispatch(Action action)
        {
            Enqueue(action);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task Enqueue(Func<Task> action)
        {
            return Enqueue(action, CancellationToken.None);
        }

        /// <summary>
        /// Enqueue the function respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return Enqueue(action, CancellationToken.None);
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task Enqueue(Action action)
        {
            using (await _AwaitableConstraint.WaitForReadiness(CancellationToken.None))
            {
                action();
            }
        }

        /// <summary>
        /// Enqueue the function respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<T> Enqueue<T>(Func<T> action)
        {
            using (await _AwaitableConstraint.WaitForReadiness(CancellationToken.None))
            {
                return action();
            }
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken))
            {
                await action();
            }
        }

        /// <summary>
        /// Enqueue the action respecting the awaitable constraint
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken))
            {
                return await action();
            }
        }
    }
}
