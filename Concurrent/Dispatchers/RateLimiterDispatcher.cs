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

        public async void Dispatch(Action action)
        {
            await Enqueue(action);
        }

        public async Task Enqueue(Action action)
        {
            using (await _AwaitableConstraint.WaitForReadiness(CancellationToken.None))
            {
                action();
            }
        }

        public async Task<T> Enqueue<T>(Func<T> action)
        {
            using (await _AwaitableConstraint.WaitForReadiness(CancellationToken.None))
            {
                return action();
            }
        }

        public Task Enqueue(Func<Task> action)
        {
            return Enqueue(action, CancellationToken.None);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            return Enqueue(action, CancellationToken.None);
        }

        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken))
            {
                await action();
            }
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            using (await _AwaitableConstraint.WaitForReadiness(cancellationToken))
            {
                return await action();
            }
        }
    }
}
