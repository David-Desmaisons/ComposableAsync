using System;

namespace ComposableAsync.Resilient.CircuitBreaker.Open
{
    public static class OpenBehaviors
    {
        private class NoThrowBehaviour : IOpenBehaviourVoid
        {
            public void OnOpen() {}
        }

        private class ThrowBehaviour : IOpenBehaviourVoid
        {
            public void OnOpen() => throw new CircuitBreakerOpenException();
        }

        private class ThrowReturnBehaviour : IOpenBehaviourReturn
        {
            public T OnOpen<T>() => throw new CircuitBreakerOpenException();
        }

        private class DefaultReturnBehaviour : IOpenBehaviourReturn
        {
            public T OnOpen<T>() => default(T);
        }

        private class ReturnBehaviour<TTarget> : IOpenBehaviourReturn
        {
            private readonly TTarget _Value;

            public ReturnBehaviour(TTarget value)
            {
                _Value = value;
            }

            public T OnOpen<T>() => typeof(T) == typeof(TTarget) ? (T)(object)_Value: throw new CircuitBreakerOpenException();
        }

        internal static IOpenBehaviourVoid ThrowVoid { get; } = new ThrowBehaviour();
        internal static IOpenBehaviourVoid NoThrowVoid { get; } = new NoThrowBehaviour();
        internal static IOpenBehaviourReturn ThrowReturn { get; } = new ThrowReturnBehaviour();
        internal static IOpenBehaviourReturn DefaultReturn { get; } = new DefaultReturnBehaviour();
        internal static IOpenBehaviourReturn Return<T>(T value) =>  new ReturnBehaviour<T>(value);
    }
}
