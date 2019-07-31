using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// Exception raised when the circuit breaker is in open state
    /// </summary>
    public class CircuitBreakerOpenException: Exception
    {
    }
}
