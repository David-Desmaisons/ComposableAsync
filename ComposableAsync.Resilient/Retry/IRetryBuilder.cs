using System;

namespace ComposableAsync.Retry
{
    public interface IRetryBuilder
    {
        IRetryBuilder And<T>() where T : Exception;

        IDispatcher ForEver();

        IDispatcher WithMaxRetry(int maxTimes);
    }
}
