using System;

namespace ComposableAsync.Resilient.ExceptionFilter
{
    internal class PredicateExceptionFilter : IExceptionFilter
    {
        private readonly Predicate<Exception> _Predicate;

        public PredicateExceptionFilter(Predicate<Exception> predicate)
        {
            this._Predicate = predicate;
        }

        public bool IsFiltered(Exception exception)
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

        public bool IsFiltered(Exception exception)
        {
            if (exception is T typedException)
                return !_Predicate(typedException);

            return true;
        }
    }
}
