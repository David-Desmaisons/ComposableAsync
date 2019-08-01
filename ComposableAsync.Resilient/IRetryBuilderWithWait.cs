using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// Retry <see cref="IDispatcher"/> builder
    /// </summary>
    public interface IRetryBuilderWithWait : IRetryBuilder
    {
        /// <summary>
        /// Set a time to wait between retries
        /// </summary>
        /// <param name="waitInMilliseconds"></param>
        /// <returns></returns>
        IRetryBuilder WithWaitBetweenRetry(int waitInMilliseconds);

        /// <summary>
        /// Set a time to wait between retries
        /// </summary>
        /// <param name="wait"></param>
        /// <returns></returns>
        IRetryBuilder WithWaitBetweenRetry(TimeSpan wait);

        /// <summary>
        /// Set a collection of times to wait between successive retries
        /// </summary>
        /// <param name="waits"></param>
        /// <returns></returns>
        IRetryBuilder WithWaitBetweenRetry(params TimeSpan[] waits);

        /// <summary>
        /// Set a collection of times to wait between successive retries
        /// </summary>
        /// <param name="waits"></param>
        /// <returns></returns>
        IRetryBuilder WithWaitBetweenRetry(params int[] waits);
    }
}
