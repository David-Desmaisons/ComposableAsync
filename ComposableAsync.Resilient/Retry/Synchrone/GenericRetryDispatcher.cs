using System;

namespace ComposableAsync.Retry
{
    internal sealed class GenericRetryDispatcher : RetryDispatcherBase
    {
        public GenericRetryDispatcher(int maxRetry) : base(maxRetry)
        {
        }

        protected override void RethrowIfNeeded(Exception exception)
        {
        }
    }
}
