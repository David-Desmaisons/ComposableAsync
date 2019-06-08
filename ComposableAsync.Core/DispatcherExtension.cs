using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using ComposableAsync.Awaitable;

namespace ComposableAsync
{
    /// <summary>
    /// <see cref="IDispatcher"/> extension methods provider
    /// </summary>
    public static class DispatcherExtension
    {
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
        /// Return a cancellable awaitable linked to the corresponding ICancellableDispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Corresponding awaitable struct</returns>
        public static DispatcherCancelableAwaitable ConfigureAwait(this ICancellableDispatcher dispatcher, CancellationToken cancellationToken)
        {
            var awaiter = new DispatcherCancelableAwaiter(dispatcher, cancellationToken);
            return new DispatcherCancelableAwaitable(awaiter);
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

        /// <summary>
        /// Create a <see cref="DelegatingHandler"/> from an <see cref="ICancellableDispatcher"/>
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public static DelegatingHandler AsDelegatingHandler(this ICancellableDispatcher dispatcher)
        {
            return new CancellableDispatcherDelegatingHandler(dispatcher);
        }

        /// <summary>
        /// Create a <see cref="DelegatingHandler"/> from an <see cref="IDispatcher"/>
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public static DelegatingHandler AsDelegatingHandler(this IDispatcher dispatcher)
        {
            return new DispatcherDelegatingHandler(dispatcher);
        }
    }
}
