using RateLimiter;

namespace Concurrent.Dispatchers
{
    /// <summary>
    /// Extension methods for <see cref="IAwaitableConstraint"/> 
    /// </summary>
    public static class AwaitableConstraintExtensions
    {
        /// <summary>
        /// Transform a <see cref="IAwaitableConstraint"/> into a <see cref="ICancellableDispatcher"/>
        /// </summary>
        /// <param name="awaitableConstraint"></param>
        /// <returns></returns>
        public static ICancellableDispatcher ToDispatcher(this IAwaitableConstraint awaitableConstraint) 
            => new RateLimiterDispatcher(awaitableConstraint);
    }
}
