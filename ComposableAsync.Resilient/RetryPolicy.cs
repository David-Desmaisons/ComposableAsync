using ComposableAsync.Retry;
using System;

namespace ComposableAsync
{
    public static class RetryPolicy
    {
        public static IRetryBuilder For<T>() where T : Exception
        {
            return new RetryBuilder(typeof(T));
        }

        public static IRetryBuilder ForAllException()
        {
            return new RetryBuilder();
        }
    }
}
