﻿using System;
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
        /// Dispatcher awaiter, making a dispatcher awaitable
        /// </summary>
        public struct DispatcherAwaiter : INotifyCompletion
        {
            /// <summary>
            /// Dispatcher never is synchronous
            /// </summary>
            public bool IsCompleted => false;

            private readonly IDispatcher _Dispatcher;

            /// <summary>
            /// Construct a NotifyCompletion fom a dispatcher
            /// </summary>
            /// <param name="dispatcher"></param>
            public DispatcherAwaiter(IDispatcher dispatcher)
            {
                _Dispatcher = dispatcher;
            }

            /// <summary>
            /// Dispatch on complete
            /// </summary>
            /// <param name="continuation"></param>
            [SecuritySafeCritical]
            public void OnCompleted(Action continuation)
            {
                _Dispatcher.Dispatch(continuation);
            }

            /// <summary>
            /// No Result
            /// </summary>
            public void GetResult() { }
        }

        /// <summary>
        /// Returns awaitable to enter in the dispatcher context
        /// This extension method make a dispatcher awaitable
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public static DispatcherAwaiter GetAwaiter(this IDispatcher dispatcher)
        {
            return new DispatcherAwaiter(dispatcher);
        }
    }
}
