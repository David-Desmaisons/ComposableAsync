using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// Circuit breaker Dispatcher factory
    /// </summary>
    public interface ICircuitBreakerWithOpenPolicyBuilder : ICircuitBreakerBuilder
    {
        ICircuitBreakerWithOpenPolicyBuilder ReturnsDefaultWhenOpen();

        ICircuitBreakerWithOpenPolicyBuilder ReturnsWhenOpen<T>(T value);

        ICircuitBreakerWithOpenPolicyBuilder DoNotThrowForVoid();
    }
}
