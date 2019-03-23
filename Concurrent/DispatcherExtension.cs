using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace Concurrent
{
    /// <summary>
    /// <see cref="IDispatcher"/> extension methods provider
    /// </summary>
    public static class DispatcherExtension
    {
        /// <summary>
        /// Dispatcher awaiter
        /// </summary>
        public struct DispatcherAwaiter : INotifyCompletion
        {
            public bool IsCompleted => false;

            private readonly IDispatcher _Dispatcher;

            public DispatcherAwaiter(IDispatcher dispatcher)
            {
                _Dispatcher = dispatcher;
            }

            [SecuritySafeCritical]
            public void OnCompleted(Action continuation)
            {
                _Dispatcher.Dispatch(continuation);
            }

            public void GetResult() { }
        }

        /// <summary>
        /// DispatcherAwaiter provider
        /// </summary>
        public struct DispatcherAwaiterProvider
        {
            private readonly DispatcherAwaiter _Awaiter;
            public DispatcherAwaiterProvider(IDispatcher fiber)
            {
                _Awaiter = new DispatcherAwaiter(fiber);
            }

            public DispatcherAwaiter GetAwaiter() => _Awaiter;
        }

        /// <summary>
        /// Returns awaitable to enter in the dispatcher context
        /// Useful to await a dispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public static DispatcherAwaiterProvider SwitchToContext(this IDispatcher dispatcher)
        {
            return new DispatcherAwaiterProvider(dispatcher);
        }
    }
}
