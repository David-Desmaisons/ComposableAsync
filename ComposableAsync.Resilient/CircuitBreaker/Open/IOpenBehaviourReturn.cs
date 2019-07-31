namespace ComposableAsync.Resilient.CircuitBreaker.Open
{
    internal interface IOpenBehaviourReturn
    {
        T OnOpen<T>();
    }
}
