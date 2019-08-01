using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// Retry <see cref="IDispatcher"/> builder
    /// </summary>
    public interface IRetryBuilder
    {
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
