using ComposableAsync.Retry;
using System;
using ComposableAsync.Retry.ExceptionFilter;

namespace ComposableAsync
{
    /// <summary>
    /// <see cref="IRetryBuilder"/> static factory
    /// </summary>
    public static class RetryPolicy
    {
        /// <summary>
        /// Creates a <see cref="IRetryWithTypeBuilder"/> that will caught the specified exception type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IRetryWithTypeBuilder For<T>() where T : Exception
        {
            return new RetryWithTypeBuilder(typeof(T));
        }

        /// <summary>
        /// Creates a <see cref="IRetryBuilder"/> that will caught all exceptions
        /// </summary>
        /// <returns></returns>
        public static IRetryBuilder ForAllException()
        {
            return new RetryBuilder(NoThrow.Instance);
        }

        /// <summary>
        /// Returns a <see cref="IRetryBuilder"/> for the given predicate and exception type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IRetryBuilder For<T>(Predicate<T> filter) where T : Exception
        {
            return new RetryBuilder(new PredicateExceptionFilter<T>(filter));
        }

        /// <summary>
        /// Returns a <see cref="IRetryBuilder"/> for the given predicate and exception type
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IRetryBuilder ForException(Predicate<Exception> filter)
        {
            return new RetryBuilder(new PredicateExceptionFilter(filter));
        }
    }
}
