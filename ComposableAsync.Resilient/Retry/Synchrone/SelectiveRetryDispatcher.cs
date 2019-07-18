using System;
using System.Collections.Generic;
using System.Linq;

namespace ComposableAsync.Retry
{
    internal sealed class SelectiveRetryDispatcher : RetryDispatcherBase
    {
        private readonly HashSet<Type> _Types;

        internal SelectiveRetryDispatcher(HashSet<Type> types, int maxRetry) : base(maxRetry)
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
