using System;
using ComposableAsync.Resilient.ExceptionFilter;
using ComposableAsync.Resilient.Retry;

namespace ComposableAsync.Resilient
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
        /// Creates a <see cref="IRetryBuilderWithWait"/> that will caught all exceptions
        /// </summary>
        /// <returns></returns>
        public static IRetryBuilderWithWait ForAllException()
        {
            return new RetryBuilderWithWait(NoThrow.Instance);
        }

        /// <summary>
        /// Returns a <see cref="IRetryBuilderWithWait"/> for the given predicate and exception type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IRetryBuilderWithWait For<T>(Predicate<T> filter) where T : Exception
        {
            return new RetryBuilderWithWait(new PredicateExceptionFilter<T>(filter));
        }

        /// <summary>
        /// Returns a <see cref="IRetryBuilderWithWait"/> for the given predicate and exception type
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IRetryBuilderWithWait ForException(Predicate<Exception> filter)
        {
            return new RetryBuilderWithWait(new PredicateExceptionFilter(filter));
        }
    }
}
