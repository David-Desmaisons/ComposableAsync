using System;
using System.Collections.Generic;
using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.Retry
{
    internal sealed class RetryWithTypeBuilder : IRetryWithTypeBuilder
    {
        private readonly HashSet<Type> _Type = new HashSet<Type>();
        private readonly ThrowOnType _ThrowOnType;

        internal RetryWithTypeBuilder(Type type)
        {
            _ThrowOnType = new ThrowOnType(_Type);
            _Type.Add(type);
        }

        public IRetryWithTypeBuilder And<T>() where T : Exception
        {
            _Type.Add(typeof(T));
            return this;
        }

        private IRetryBuilderWithWait GetRetryBuilder() => new RetryBuilderWithWait(_ThrowOnType);

        public IRetryBuilder WithWaitBetweenRetry(int waitInMilliseconds)
            => GetRetryBuilder().WithWaitBetweenRetry(waitInMilliseconds);

        public IRetryBuilder WithWaitBetweenRetry(TimeSpan wait)
            => GetRetryBuilder().WithWaitBetweenRetry(wait);

        public IRetryBuilder WithWaitBetweenRetry(params TimeSpan[] waits)
            => GetRetryBuilder().WithWaitBetweenRetry(waits);

        public IRetryBuilder WithWaitBetweenRetry(params int[] waits)
            => GetRetryBuilder().WithWaitBetweenRetry(waits);

        public IDispatcher ForEver() => GetRetryBuilder().ForEver();

        public IDispatcher WithMaxRetry(int maxTimes) => GetRetryBuilder().WithMaxRetry(maxTimes);
    }
}
