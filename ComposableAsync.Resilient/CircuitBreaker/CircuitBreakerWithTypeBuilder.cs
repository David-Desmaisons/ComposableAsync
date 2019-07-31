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

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, TimeSpan timeoutBeforeRetry)
        {
            return GetCircuitBreakerBuilder().WithRetryAndTimeout(attemptsBeforeOpen, timeoutBeforeRetry);
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, int timeoutBeforeRetryInMilliseconds)
        {
            return GetCircuitBreakerBuilder().WithRetryAndTimeout(attemptsBeforeOpen, timeoutBeforeRetryInMilliseconds);
        }

        private ICircuitBreakerWithOpenPolicyBuilder GetCircuitBreakerBuilder() => new CircuitBreakerWithOpenPolicyBuilder(_ThrowOnType);

        public ICircuitBreakerWithOpenPolicyBuilder ReturnsDefaultWhenOpen()
        {
            return GetCircuitBreakerBuilder().ReturnsDefaultWhenOpen();
        }

        public ICircuitBreakerWithOpenPolicyBuilder ReturnsWhenOpen<T>(T value)
        {
            return GetCircuitBreakerBuilder().ReturnsWhenOpen<T>(value);
        }

        public ICircuitBreakerWithOpenPolicyBuilder DoNotThrowForVoid()
        {
            return GetCircuitBreakerBuilder().DoNotThrowForVoid();
        }
    }
}
