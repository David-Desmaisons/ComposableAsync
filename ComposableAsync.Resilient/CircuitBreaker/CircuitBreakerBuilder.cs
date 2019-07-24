using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerBuilder : ICircuitBreakerBuilder
    {
        private readonly IExceptionFilter _ExceptionFilter;

        internal CircuitBreakerBuilder(IExceptionFilter exceptionFilter)
        {
            _ExceptionFilter = exceptionFilter;
        }

        public IDispatcher From()
        {
            throw new System.NotImplementedException();
        }
    }
}
