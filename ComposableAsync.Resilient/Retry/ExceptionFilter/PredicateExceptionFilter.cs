using System;

namespace ComposableAsync.Resilient.Retry.ExceptionFilter
{
    internal class PredicateExceptionFilter : IExceptionFilter
    {
        private readonly Predicate<Exception> _Predicate;

        public PredicateExceptionFilter(Predicate<Exception> predicate)
        {
            _Predicate = predicate;
        }

        public bool ShouldBeThrown(Exception exception)
        {
            return !_Predicate(exception);
        }
    }

    internal class PredicateExceptionFilter<T> : IExceptionFilter where T : Exception
    {
        private readonly Predicate<T> _Predicate;

        public PredicateExceptionFilter(Predicate<T> predicate)
        {
            _Predicate = predicate;
        }

        public bool ShouldBeThrown(Exception exception)
        {
            if (exception is T typedException)
                return !_Predicate(typedException);

            return true;
        }
    }
}
