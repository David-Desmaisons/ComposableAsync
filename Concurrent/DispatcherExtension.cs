using System;
using System.Runtime.CompilerServices;
using System.Security;
using Concurrent.Dispatchers;
using RateLimiter;

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

        /// <summary>
        /// Returns a composed dispatcher applying the given dispatcher
        /// after the first one
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static IDispatcher Then(this IDispatcher dispatcher, IDispatcher other)
        {
            return new ComposedDispatcher(dispatcher, other);
        }

        /// <summary>
        /// Returns a composed dispatcher applying the given dispatcher
        /// after the first one
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static ICancellableDisposableDispatcher Then(this ICancellableDispatcher dispatcher, ICancellableDispatcher other)
        {
            return new ComposedCancellableDispatcher(dispatcher, other);
        }

        /// <summary>
        /// Returns a composed dispatcher applying the given dispatcher
        /// after the first one
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static ICancellableDisposableDispatcher Then(this IRateLimiter constraint, ICancellableDispatcher other)
        {
            return new ComposedCancellableDispatcher(constraint.ToDispatcher(), other);
        }

        /// <summary>
        /// Returns a composed dispatcher applying the given dispatcher
        /// after the first one
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="constraint"></param>
        /// <returns></returns>
        public static ICancellableDisposableDispatcher Then(this ICancellableDispatcher dispatcher, IRateLimiter constraint)
        {
            return new ComposedCancellableDispatcher(dispatcher, constraint.ToDispatcher());
        }
    }
}
