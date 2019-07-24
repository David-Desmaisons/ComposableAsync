using System;
using System.Collections.Generic;
using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerWithTypeBuilder : ICircuitBreakerWithTypeBuilder
    {
        private readonly HashSet<Type> _Type = new HashSet<Type>();
        private readonly ThrowOnType _ThrowOnType;

        internal CircuitBreakerWithTypeBuilder(Type type)
        {
            _ThrowOnType = new ThrowOnType(_Type);
            _Type.Add(type);
        }

        public ICircuitBreakerWithTypeBuilder And<T>() where T : Exception
        {
            _Type.Add(typeof(T));
            return this;
        }

        public IDispatcher From()
        {
            return new CircuitBreakerBuilder(_ThrowOnType).From();
        }
    }
}
