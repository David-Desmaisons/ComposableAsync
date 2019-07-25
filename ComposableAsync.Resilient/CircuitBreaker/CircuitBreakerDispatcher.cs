using ComposableAsync.Resilient.ExceptionFilter;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerDispatcher : IBasicDispatcher
    {
        private readonly IExceptionFilter _ExceptionFilter;
        private readonly int _Threshold;
        private readonly TimeSpan _Delay;

        private BreakerState _State = BreakerState.Closed;
        private DateTime _LastFail;
        private int _SuccessiveFails = 0;

        public CircuitBreakerDispatcher(IExceptionFilter exceptionFilter, int threshold, TimeSpan delay)
        {
            _ExceptionFilter = exceptionFilter;
            _Threshold = threshold;
            _Delay = delay;
        }

        public IBasicDispatcher Clone()
        {
            return new CircuitBreakerDispatcher(_ExceptionFilter, _Threshold, _Delay);
        }

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            OnEnter();
            T res;
            try
            {
                res = action();
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
            OnSuccess();
            return Task.FromResult(res);
        }

        public Task Enqueue(Action action, CancellationToken cancellationToken)
        {
            OnEnter();
            try
            {
                action();
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
            OnSuccess();
            return Task.CompletedTask;
        }

        public async Task Enqueue(Func<Task> action, CancellationToken cancellationToken)
        {
            OnEnter();
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
            OnSuccess();
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            OnEnter();
            T res;
            try
            {
                res = await action();
            }
            catch (Exception exception)
            {
                CheckException(exception);
                throw;
            }
            OnSuccess();
            return res;
        }

        private void CheckException(Exception exception)
        {
            if (!_ExceptionFilter.IsFiltered(exception))
                return;

            lock (this)
            {
                UnsafeUpdateOnFail();
            }
        }

        private void UnsafeUpdateOnFail()
        {
            _LastFail = DateTime.Now;
            switch (_State)
            {
                case BreakerState.Open:
                case BreakerState.HalfOpen:
                    _State = BreakerState.Open;
                    return;

                case BreakerState.Closed:
                    if (_SuccessiveFails++ == _Threshold)
                        _State = BreakerState.Open;
                    return;
            }
        }

        private void OnEnter()
        {
            lock (this)
            {
                UnsafeEnter();
            }
        }

        private void UnsafeEnter()
        {
            switch (_State)
            {
                case BreakerState.Closed:
                    return;

                case BreakerState.HalfOpen:
                    throw new CircuitBreakerOpenException();

                case BreakerState.Open:
                    var now = DateTime.Now;
                    if (now.Subtract(_LastFail) < _Delay)
                        throw new CircuitBreakerOpenException();
                    _State = BreakerState.HalfOpen;
                    return;
            }
        }

        private void OnSuccess()
        {
            lock (this)
            {
                _SuccessiveFails = 0;
                _State = BreakerState.Closed;
            }
        }
    }
}
