using System;

namespace ComposableAsync.Retry
{
    internal sealed class GenericRetryDispatcherAsync : RetryDispatcherAsyncBase
    {
        public GenericRetryDispatcherAsync(TimeSpan[] timeSpans, int maxRetry): base(maxRetry, timeSpans)
        {
        }

        protected override void RethrowIfNeeded(Exception exception)
        {
        }
    }
}