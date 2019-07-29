using System;

namespace ComposableAsync.Resilient
{
    public interface ICircuitBreakerWithTypeBuilder : ICircuitBreakerWithOpenPolicyBuilder
    {
        /// <summary>
        /// Add the exception type to be caught
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ICircuitBreakerWithTypeBuilder And<T>() where T : Exception;
    }
}
