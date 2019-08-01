using System;
using System.Collections.Generic;
using System.Linq;

namespace ComposableAsync.Resilient.Retry.TimeOuts
{
    internal class ArrayTimeOutProvider : ITimeOutProvider
    {
        private readonly TimeSpan[] _TimeSpans;

        internal ArrayTimeOutProvider(TimeSpan[] timeSpans)
        {
            _TimeSpans = timeSpans;
        }

        internal ArrayTimeOutProvider(IEnumerable<int> timeOutInMilliseconds)
        {
            _TimeSpans = timeOutInMilliseconds.Select(t => TimeSpan.FromMilliseconds(t)).ToArray();
        }

        public TimeSpan GetTimeOutForRetry(int retryNumber)
        {
            var length = _TimeSpans.Length - 1;
            return (retryNumber > length) ? _TimeSpans[length] : _TimeSpans[retryNumber];
        }
    }
}
