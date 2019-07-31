using System;

namespace ComposableAsync.Resilient.Retry.TimeOuts
{
    internal interface ITimeOutProvider
    {
        TimeSpan GetTimeOutForRetry(int retryNumber);
    }
}
