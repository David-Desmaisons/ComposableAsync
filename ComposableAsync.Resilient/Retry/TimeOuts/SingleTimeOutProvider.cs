using System;

namespace ComposableAsync.Resilient.Retry.TimeOuts
{
    internal class SingleTimeOutProvider : ITimeOutProvider
    {
        private readonly TimeSpan _TimeSpan;

        internal SingleTimeOutProvider(TimeSpan timeSpan)
        {
            _TimeSpan = timeSpan;
        }

        internal SingleTimeOutProvider(int milliseconds)
        {
            _TimeSpan = TimeSpan.FromMilliseconds(milliseconds);
        }

        public TimeSpan GetTimeOutForRetry(int retryNumber) => _TimeSpan;
    }
}
