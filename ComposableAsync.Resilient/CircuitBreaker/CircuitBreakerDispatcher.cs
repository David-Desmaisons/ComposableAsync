using ComposableAsync.Resilient.CircuitBreaker.Open;
using ComposableAsync.Resilient.ExceptionFilter;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerDispatcher : IBasicDispatcher
    {
        private readonly IExceptionFilter _ExceptionFilter;
        private readonly IOpenBehaviourVoid _OpenBehaviour;
        private readonly IOpenBehaviourReturn _OpenBehaviourReturn;
        private readonly int _Threshold;
        private readonly TimeSpan _Delay;

        private BreakerState _State = BreakerState.Closed;
        private DateTime _LastFail;
        private int _SuccessiveFails = 0;

        public CircuitBreakerDispatcher(IExceptionFilter exceptionFilter,  IOpenBehaviourVoid openBehaviour, IOpenBehaviourReturn openBehaviourReturn, int threshold, TimeSpan delay)
        {
            _ExceptionFilter = exceptionFilter;
            _Threshold = threshold;
            _Delay = delay;
            _OpenBehaviour = openBehaviour;
            _OpenBehaviourReturn = openBehaviourReturn;
        }

        public IBasicDispatcher Clone() => new CircuitBreakerDispatcher(_ExceptionFilter, _OpenBehaviour, _OpenBehaviourReturn, _Threshold, _Delay);

        public Task<T> Enqueue<T>(Func<T> action, CancellationToken cancellationToken)
        {
            var result = OnEnter<T>();
            if (!result.Continue)
                return Task.FromResult(result.Value);

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
            if (!OnEnter())
                return Task.CompletedTask;

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
            if (!OnEnter())
                return;
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
            var result = OnEnter<T>();
            if (!result.Continue)
                return result.Value;

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
            if (_ExceptionFilter.IsFiltered(exception))
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
                    if (++_SuccessiveFails == _Threshold)
                        _State = BreakerState.Open;
                    return;
            }
        }

        private bool OnEnter()
        {
            lock (this)
            {
                return UnsafeEnter();
            }
        }

        private ReturnData<T> OnEnter<T>()
        {
            lock (this)
            {
                return UnsafeEnter<T>();
            }
        }

        private class ReturnData<T>
        {
            public bool Continue { get; }
            public T Value { get; }

            private ReturnData()
            {
                Continue = true;
            }

            private ReturnData(T value)
            {
                Continue = false;
                Value = value;
            }

            public static ReturnData<T> GetContinue() => new ReturnData<T>();
            public static ReturnData<T> Return(T value) => new ReturnData<T>(value);
        }

        private ReturnData<T> UnsafeEnter<T>()
        {
            return UnsafeEnter(ReturnData<T>.GetContinue, () => ReturnData<T>.Return(_OpenBehaviourReturn.OnOpen<T>()));
        }

        private bool UnsafeEnter()
        {
            return UnsafeEnter(() => true, () =>
            {
                _OpenBehaviour.OnOpen();
                return false;
            });
        }

        private T UnsafeEnter<T>(Func<T> onClosed, Func<T> onOpen)
        {
            switch (_State)
            {
                case BreakerState.Closed:
                    return onClosed();

                case BreakerState.HalfOpen:
                    _OpenBehaviour.OnOpen();
                    return onOpen();

                case BreakerState.Open:
                    var now = DateTime.Now;
                    if (now.Subtract(_LastFail) < _Delay)
                        return onOpen();
                    _State = BreakerState.HalfOpen;
                    return onClosed();

                default:
                    throw new Exception();
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
