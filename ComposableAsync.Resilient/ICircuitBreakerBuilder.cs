using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// Circuit breaker Dispatcher factory
    /// </summary>
    public interface ICircuitBreakerBuilder
    {
        /// <summary>
        /// Create a Circuit breaker Dispatcher
        /// </summary>
        /// <param name="attemptsBeforeOpen"></param>
        /// <param name="timeoutBeforeRetry"></param>
        /// <returns></returns>
        IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, TimeSpan timeoutBeforeRetry);

        /// <summary>
        /// Create a Circuit breaker Dispatcher
        /// </summary>
        /// <param name="attemptsBeforeOpen"></param>
        /// <param name="timeoutBeforeRetryInMilliseconds"></param>
        /// <returns></returns>
        IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, int timeoutBeforeRetryInMilliseconds);
    }
}
