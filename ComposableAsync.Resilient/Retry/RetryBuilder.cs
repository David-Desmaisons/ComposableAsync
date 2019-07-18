using System;
using System.Collections.Generic;
using System.Linq;
using ComposableAsync.Retry.ExceptionFilter;

namespace ComposableAsync.Retry
{
    internal sealed class RetryBuilder: IRetryBuilder
    {
        private readonly bool _All;
        private readonly HashSet<Type> _Type = new HashSet<Type>();
        private readonly List<TimeSpan> _Waits = new List<TimeSpan>();

        public RetryBuilder()
        {
            _All = true;
        }

        public RetryBuilder(Type type)
        {
            _Type.Add(type);
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
            var filter = GetExceptionFilter();
            return (_Waits.Count > 0) ? (IBasicDispatcher) new RetryDispatcherAsync(filter, max, _Waits.ToArray()) :
                new RetryDispatcher(filter, max);
        }

        private IExceptionFilter GetExceptionFilter()
        {
            return _All ? (IExceptionFilter) new NoThrow() : new ThrowOnType(_Type);
        }
    }
}
