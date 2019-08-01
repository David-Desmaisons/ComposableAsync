using ComposableAsync.Resilient.ExceptionFilter;
using ComposableAsync.Resilient.Retry.TimeOuts;

namespace ComposableAsync.Resilient.Retry
{
    internal sealed class RetryBuilder : IRetryBuilder
    {
        private readonly IExceptionFilter _ExceptionFilter;
        private readonly ITimeOutProvider _TimeOutProvider;

        internal RetryBuilder(IExceptionFilter exceptionFilter, ITimeOutProvider timeOutProvider = null)
        {
            _ExceptionFilter = exceptionFilter;
            _TimeOutProvider = timeOutProvider;
        }

        public IDispatcher ForEver() => GetBasicDispatcher(int.MaxValue).ToFullDispatcher();

        public IDispatcher WithMaxRetry(int maxTimes) => GetBasicDispatcher(maxTimes).ToFullDispatcher();

        private IBasicDispatcher GetBasicDispatcher(int max)
        {
            return (_TimeOutProvider != null) ? (IBasicDispatcher)new RetryDispatcherAsync(_ExceptionFilter, _TimeOutProvider, max) :
                new RetryDispatcher(_ExceptionFilter, max);
        }
    }
}
