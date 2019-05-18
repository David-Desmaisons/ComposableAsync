using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;

namespace ComposableAsync
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
        public static ICancellableDispatcher Then(this ICancellableDispatcher dispatcher, ICancellableDispatcher other)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return new ComposedCancellableDispatcher(dispatcher, other);
        }

        /// <summary>
        /// Returns a composed dispatcher applying the given dispatchers sequentially
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static ICancellableDispatcher Then(this ICancellableDispatcher dispatcher, params ICancellableDispatcher[] others)
        {
            return dispatcher.Then((IEnumerable<ICancellableDispatcher>)others);
        }

        /// <summary>
        /// Returns a composed dispatcher applying the given dispatchers sequentially
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static ICancellableDispatcher Then(this ICancellableDispatcher dispatcher, IEnumerable<ICancellableDispatcher> others)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));

            if (others == null)
                throw new ArgumentNullException(nameof(others));

            return others.Aggregate(dispatcher, (cum, val) => cum.Then(val));
        }
    }
}
