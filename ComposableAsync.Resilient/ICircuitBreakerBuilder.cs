namespace ComposableAsync.Resilient
{
    public interface ICircuitBreakerBuilder
    {
        IDispatcher From();
    }
}
