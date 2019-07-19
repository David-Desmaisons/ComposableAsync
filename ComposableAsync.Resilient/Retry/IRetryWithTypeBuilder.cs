using System;

namespace ComposableAsync.Retry
{
    /// <summary>
    /// Retry <see cref="IDispatcher"/> builder
    /// </summary>
    public interface IRetryWithTypeBuilder : IRetryBuilder
    {
        /// <summary>
        /// Add the exception type to be caught
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IRetryWithTypeBuilder And<T>() where T : Exception;
    }
}
