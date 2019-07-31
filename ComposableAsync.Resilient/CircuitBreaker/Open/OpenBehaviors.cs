using System;
using System.Collections.Generic;

namespace ComposableAsync.Resilient.CircuitBreaker.Open
{
    internal static class OpenBehaviors
    {
        private class NoThrowBehaviour : IOpenBehaviourVoid
        {
            public void OnOpen() { }
        }

        private class ThrowBehaviour : IOpenBehaviourVoid
        {
            public void OnOpen() => throw new CircuitBreakerOpenException();
        }

        private class ThrowReturnBehaviour : IOpenBehaviourReturn
        {
            public T OnOpen<T>() => throw new CircuitBreakerOpenException();
        }

        private class ReturnBehaviour : IOpenBehaviourReturn
        {
            private readonly Dictionary<Type, object> _Values;
            private readonly bool _UseDefault;

            public ReturnBehaviour(Dictionary<Type, object> values, bool useDefault)
            {
                _Values = values;
                _UseDefault = useDefault;
            }

            public T OnOpen<T>()
            {
                if (_Values.TryGetValue(typeof(T), out var res))
                    return (T)res;

                return _UseDefault ? default(T) : throw new CircuitBreakerOpenException();
            }
        }

        private static IOpenBehaviourReturn ThrowReturn { get; } = new ThrowReturnBehaviour();

        internal static IOpenBehaviourVoid ThrowVoid { get; } = new ThrowBehaviour();
        internal static IOpenBehaviourVoid NoThrowVoid { get; } = new NoThrowBehaviour();
        internal static IOpenBehaviourReturn Return(Dictionary<Type, object> values, bool useDefault) =>
            (!useDefault && values.Count == 0) ? ThrowReturn : new ReturnBehaviour(values, useDefault);
    }
}
