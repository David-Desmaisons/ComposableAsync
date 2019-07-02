﻿using System.Diagnostics;
using Castle.DynamicProxy;

namespace ComposableAsync.Factory.Proxy 
{
    [DebuggerNonUserCode]
    internal struct ProxyFiberSolver 
    {
        internal bool Continue { get; }

        private InvocationOnDispatcher Transform { get; }

        internal object Invoke(IDispatcher dispatcher, IInvocation invocation) =>
            Transform.Invoke(dispatcher, invocation);

        internal ProxyFiberSolver(InvocationOnDispatcher transform, bool @continue)
        {
            Continue = @continue;
            Transform = transform;
        }
    }
}
