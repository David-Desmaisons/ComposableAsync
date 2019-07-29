namespace ComposableAsync.Resilient.CircuitBreaker.Open
{
    public interface IOpenBehaviourReturn
    {
        T OnOpen<T>();
    }
}
