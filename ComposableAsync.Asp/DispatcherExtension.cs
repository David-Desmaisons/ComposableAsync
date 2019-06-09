using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;

namespace ComposableAsync.Asp
{
    public static class DispatcherExtension
    {
        public static void UseDispatcher(IApplicationBuilder app, ICancellableDispatcher dispatcher)
        {
            app.Use((context, next) => dispatcher.Enqueue(next));
        }

        public static void UseDispatcher(IApplicationBuilder app, Func<HttpContext,ICancellableDispatcher> dispatcherFinder)
        {
            app.Use((context, next) =>
            {
                var dispatcher = dispatcherFinder(context);
                return dispatcher.Enqueue(next);
            });
        }

        public static void UseDispatcherByRemoteIpAddress(IApplicationBuilder app, Func<ICancellableDispatcher> dispatcherFinder)
        {
            var cache = new Dictionary<IPAddress, ICancellableDispatcher>();
            app.Use((context, next) =>
            {
                var dispatcher = GetOrCreate(cache, context.Connection.RemoteIpAddress, dispatcherFinder);
                return dispatcher.Enqueue(next);
            });
        }

        private static ICancellableDispatcher GetOrCreate(IDictionary<IPAddress, ICancellableDispatcher> cache, IPAddress address, Func<ICancellableDispatcher> dispatcherFinder)
        {
            if (cache.TryGetValue(address, out var dispatcher))
                return dispatcher;

            dispatcher = dispatcherFinder();
            cache.Add(address, dispatcher);
            return dispatcher;
        }
    }
}
