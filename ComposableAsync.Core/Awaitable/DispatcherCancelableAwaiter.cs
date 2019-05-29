using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;

namespace ComposableAsync.Awaitable
{
    /// <summary>
    /// Dispatcher awaiter, making a dispatcher awaitable
    /// </summary>
    public struct DispatcherCancelableAwaiter : INotifyCompletion
    {
        /// <summary>
        /// Dispatcher never is synchronous
        /// </summary>
        public bool IsCompleted => false;

        private readonly ICancellableDispatcher _Dispatcher;
        private readonly CancellationToken _CancellationToken;

        /// <summary>
        /// Construct a NotifyCompletion fom a dispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        public DispatcherCancelableAwaiter(ICancellableDispatcher dispatcher, CancellationToken cancellationToken)
        {
            _Dispatcher = dispatcher;
            _CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Dispatch on complete
        /// </summary>
        /// <param name="continuation"></param>
        [SecuritySafeCritical]
        public void OnCompleted(Action continuation)
        {
            _Dispatcher.Enqueue(continuation, _CancellationToken);
        }

        /// <summary>
        /// No Result
        /// </summary>
        public void GetResult() { }
    }
}
