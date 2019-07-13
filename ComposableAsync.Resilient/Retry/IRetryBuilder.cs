using System;

namespace ComposableAsync.Retry
{
    public interface IRetryBuilder
    {
        IRetryBuilder And<T>() where T : Exception;

        IDispatcher ForEver();

        IDispatcher Until(int maxTimes);
    }
}
