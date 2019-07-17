using System;

namespace ComposableAsync.Retry
{
    internal sealed class GenericRetryDispatcher : RetryDispatcherBase, IBasicDispatcher
    {
        public GenericRetryDispatcher(int maxRetry) : base(maxRetry)
        {
        }

        public IBasicDispatcher Clone()
        {
            return this;
        }

        protected override void RethrowIfNeeded(Exception exception)
        {
        }
    }
}
