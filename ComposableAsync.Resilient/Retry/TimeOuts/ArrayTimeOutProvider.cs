using System;

namespace ComposableAsync.Resilient.Retry.TimeOuts
{
    internal class ArrayTimeOutProvider : ITimeOutProvider
    {
        private readonly TimeSpan[] _TimeSpans;

        internal ArrayTimeOutProvider(TimeSpan[] timeSpans)
        {
            _TimeSpans = timeSpans;
        }

        public TimeSpan GetTimeOutForRetry(int retryNumber)
        {
            var length = _TimeSpans.Length - 1;
            return (retryNumber > length) ? _TimeSpans[length] : _TimeSpans[retryNumber];
        }
    }
}
