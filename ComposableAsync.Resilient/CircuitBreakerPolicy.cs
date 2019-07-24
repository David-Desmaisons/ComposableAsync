using ComposableAsync.Resilient.CircuitBreaker;
using ComposableAsync.Resilient.ExceptionFilter;
using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// <see cref="ICircuitBreakerBuilder"/> static factory
    /// </summary>
    public static class CircuitBreakerPolicy
    {
        /// <summary>
        /// Creates a <see cref="IRetryWithTypeBuilder"/> that will caught the specified exception type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ICircuitBreakerWithTypeBuilder For<T>() where T : Exception
        {
            return new CircuitBreakerWithTypeBuilder(typeof(T));
        }

        /// <summary>
        /// Creates a <see cref="IRetryBuilder"/> that will caught all exceptions
        /// </summary>
        /// <returns></returns>
        public static ICircuitBreakerBuilder ForAllException()
        {
            return new CircuitBreakerBuilder(NoThrow.Instance);
        }
    }
}
