using System;

namespace ComposableAsync.Retry
{
    /// <summary>
    /// Retry <see cref="IDispatcher"/> builder
    /// </summary>
    public interface IRetryBuilder
    {
        /// <summary>
        /// Add the exception type to be caught
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IRetryBuilder And<T>() where T : Exception;

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

        /// <summary>
        /// create a <see cref="IDispatcher"/> that will perform
        /// function till no exception without limit
        /// </summary>
        /// <returns></returns>
        IDispatcher ForEver();

        /// <summary>
        /// create a <see cref="IDispatcher"/> that will perform
        /// function till no exception or maxTimes count is reached
        /// </summary>
        /// <param name="maxTimes"></param>
        /// <returns></returns>
        IDispatcher WithMaxRetry(int maxTimes);
    }
}
