using ComposableAsync.Retry;
using System;

namespace ComposableAsync
{
    /// <summary>
    /// <see cref="IRetryBuilder"/> static factory
    /// </summary>
    public static class RetryPolicy
    {
        /// <summary>
        /// Creates a <see cref="IRetryBuilder"/> that will caught the specified exception type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IRetryBuilder For<T>() where T : Exception
        {
            return new RetryBuilder(typeof(T));
        }

        /// <summary>
        /// Creates a <see cref="IRetryBuilder"/> that will caught all exceptions
        /// </summary>
        /// <returns></returns>
        public static IRetryBuilder ForAllException()
        {
            return new RetryBuilder();
        }
    }
}
