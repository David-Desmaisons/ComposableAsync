using System;
using System.Collections.Generic;
using System.Linq;

namespace ComposableAsync.Retry
{
    internal sealed class SelectiveRetryDispatcherAsync : RetryDispatcherAsyncBase
    {
        private readonly HashSet<Type> _Types;

        internal SelectiveRetryDispatcherAsync(TimeSpan[] timeSpans, HashSet<Type> types, int maxRetry) : base(maxRetry, timeSpans)
        {
            _Types = types;
        }

        protected override void RethrowIfNeeded(Exception exception)
        {
            var type = exception.GetType();
            if (_Types.Any(t => t == type || type.IsSubclassOf(t)))
                return;

            throw exception;
        }
    }
}
