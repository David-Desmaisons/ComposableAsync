namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal enum BreakerState
    {      
        Closed,
        Open,
        HalfOpen
    }
}
