﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;

namespace ComposableAsync.Asp
{
    public static class DispatcherExtension
    {
        public static void UseDispatcher(this IApplicationBuilder app, ICancellableDispatcher dispatcher)
        {
            app.Use((context, next) => dispatcher.Enqueue(next));
        }

        public static void UseDispatcher(this IApplicationBuilder app, Func<HttpContext,ICancellableDispatcher> dispatcherFinder)
        {
            app.Use((context, next) =>
            {
                var dispatcher = dispatcherFinder(context);
                return dispatcher.Enqueue(next);
            });
        }

        public static void UseDispatcherByRemoteIpAddress(this IApplicationBuilder app, Func<ICancellableDispatcher> dispatcherFinder)
        {
            var cache = new Dictionary<IPAddress, ICancellableDispatcher>();
            app.Use((context, next) =>
            {
                var dispatcher = GetOrCreate(cache, context.Connection.RemoteIpAddress, dispatcherFinder);
                return dispatcher.Enqueue(next);
            });
        }

        public static void UseDispatcherByUser(this IApplicationBuilder app, Func<ICancellableDispatcher> dispatcherFinder)
        {
            var cache = new Dictionary<ClaimsPrincipal, ICancellableDispatcher>();
            app.Use((context, next) =>
            {
                if (context.User == null)
                    return next();

                var dispatcher = GetOrCreate(cache, context.User, dispatcherFinder);
                return dispatcher.Enqueue(next);
            });
        }

        private static ICancellableDispatcher GetOrCreate<T>(IDictionary<T, ICancellableDispatcher> cache, T address, Func<ICancellableDispatcher> dispatcherFinder)
        {
            if (cache.TryGetValue(address, out var dispatcher))
                return dispatcher;

            dispatcher = dispatcherFinder();
            cache.Add(address, dispatcher);
            return dispatcher;
        }
    }
}
