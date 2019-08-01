using System;

namespace ComposableAsync.Resilient.Retry.TimeOuts
{
    internal class FunctionTimeOutProvider : ITimeOutProvider
    {
        private readonly Func<int, TimeSpan> _Provider;
        internal FunctionTimeOutProvider(Func<int, TimeSpan> provider)
        {
            _Provider = provider;
        }

        public TimeSpan GetTimeOutForRetry(int retryNumber) => _Provider(retryNumber + 1);
    }
}
