using System;
using ComposableAsync.Resilient.ExceptionFilter;
using ComposableAsync.Resilient.Retry.TimeOuts;

namespace ComposableAsync.Resilient.Retry
{
    internal sealed class RetryBuilderWithWait : IRetryBuilderWithWait
    {
        private readonly IExceptionFilter _ExceptionFilter;

        internal RetryBuilderWithWait(IExceptionFilter exceptionFilter)
        {
            _ExceptionFilter = exceptionFilter;
        }

        public IRetryBuilder WithWaitBetweenRetry(int waitInMilliseconds)
        {
            var timeOutProvider = new SingleTimeOutProvider(waitInMilliseconds);
            return GetRetryBuilder(timeOutProvider);
        }

        public IRetryBuilder WithWaitBetweenRetry(TimeSpan wait)
        {
            var timeOutProvider = new SingleTimeOutProvider(wait);
            return GetRetryBuilder(timeOutProvider);
        }

        public IRetryBuilder WithWaitBetweenRetry(params TimeSpan[] waits)
        {
            var timeOutProvider = new ArrayTimeOutProvider(waits);
            return GetRetryBuilder(timeOutProvider);
        }

        public IRetryBuilder WithWaitBetweenRetry(params int[] waits)
        {
            var timeOutProvider = new ArrayTimeOutProvider(waits);
            return GetRetryBuilder(timeOutProvider);
        }

        public IRetryBuilder WithWaitBetweenRetry(Func<int, TimeSpan> waitProvider)
        {
            var timeOutProvider = new FunctionTimeOutProvider(waitProvider);
            return GetRetryBuilder(timeOutProvider);
        }

        public IDispatcher ForEver() => GetRetryBuilder().ForEver();

        public IDispatcher WithMaxRetry(int maxTimes) => GetRetryBuilder().WithMaxRetry(maxTimes);

        private IRetryBuilder GetRetryBuilder(ITimeOutProvider timeOutProvider=null) => new RetryBuilder(_ExceptionFilter, timeOutProvider);
    }
}
