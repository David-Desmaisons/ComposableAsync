using RateLimiter;

namespace Concurrent.Dispatchers
{
    /// <summary>
    /// Extension methods for <see cref="IAwaitableConstraint"/> 
    /// </summary>
    public static class AwaitableConstraintExtensions
    {
        /// <summary>
        /// Transform a <see cref="IRateLimiter"/> into a <see cref="ICancellableDispatcher"/>
        /// </summary>
        /// <param name="rateLimiter"></param>
        /// <returns></returns>
        public static ICancellableDispatcher ToDispatcher(this IRateLimiter rateLimiter) 
            => new RateLimiterDispatcher(rateLimiter);
    }
}
