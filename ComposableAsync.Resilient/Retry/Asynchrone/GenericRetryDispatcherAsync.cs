using System;

namespace ComposableAsync.Retry
{
    internal sealed class GenericRetryDispatcherAsync : RetryDispatcherAsyncBase, IBasicDispatcher
    {
        public GenericRetryDispatcherAsync(TimeSpan[] timeSpans, int maxRetry): base(maxRetry, timeSpans)
        {
        }

        public IBasicDispatcher Clone() => this;

        protected override void RethrowIfNeeded(Exception exception)
        {
        }
    }
}