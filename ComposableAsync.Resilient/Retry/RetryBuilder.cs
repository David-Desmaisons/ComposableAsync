using System;
using System.Collections.Generic;
using System.Linq;
using ComposableAsync.Retry.ExceptionFilter;

namespace ComposableAsync.Retry
{
    internal sealed class RetryBuilder: IRetryBuilder
    {
        private readonly HashSet<Type> _Type = new HashSet<Type>();
        private readonly List<TimeSpan> _Waits = new List<TimeSpan>();
        private readonly IExceptionFilter _ExceptionFilter;

        public RetryBuilder()
        {
            _ExceptionFilter = new NoThrow();
        }

        public RetryBuilder(Type type)
        {
            _Type.Add(type);
            _ExceptionFilter = new ThrowOnType(_Type);
        }

        public IRetryBuilder And<T>() where T : Exception
        {
            _Type.Add(typeof(T));
            return this;
        }

        public IRetryBuilder WithWaitBetweenRetry(int waitInMilliseconds)
        {
            return WithWaitBetweenRetry(TimeSpan.FromMilliseconds(waitInMilliseconds));
        }

        public IRetryBuilder WithWaitBetweenRetry(TimeSpan wait)
        {
            _Waits.Add(wait);
            return this;
        }

        public IRetryBuilder WithWaitBetweenRetry(params TimeSpan[] waits)
        {
            _Waits.AddRange(waits);
            return this;
        }

        public IRetryBuilder WithWaitBetweenRetry(params int[] waits)
        {
            _Waits.AddRange(waits.Select(w => TimeSpan.FromMilliseconds(w)));
            return this;
        }

        public IDispatcher ForEver() => GetBasicDispatcher(int.MaxValue).ToFullDispatcher();

        public IDispatcher WithMaxRetry(int maxTimes) => GetBasicDispatcher(maxTimes).ToFullDispatcher();

        private IBasicDispatcher GetBasicDispatcher(int max)
        {
            return (_Waits.Count > 0) ? (IBasicDispatcher) new RetryDispatcherAsync(_ExceptionFilter, max, _Waits.ToArray()) :
                new RetryDispatcher(_ExceptionFilter, max);
        }
    }
}
