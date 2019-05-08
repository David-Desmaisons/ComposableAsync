using System.Diagnostics;
using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy 
{
    [DebuggerNonUserCode]
    internal struct ProxyFiberSolver 
    {
        internal bool Continue { get; }

        private InvocationOnDispatcher Transform { get; }

        internal object Invoke(ICancellableDispatcher dispatcher, IInvocation invocation) =>
            Transform.Invoke(dispatcher, invocation);

        internal ProxyFiberSolver(InvocationOnDispatcher transform, bool @continue)
        {
            Continue = @continue;
            Transform = transform;
        }
    }
}
