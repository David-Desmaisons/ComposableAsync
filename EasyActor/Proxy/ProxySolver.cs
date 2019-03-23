using System.Diagnostics;

namespace EasyActor.Proxy 
{
    [DebuggerNonUserCode]
    internal struct ProxyFiberSolver 
    {
        internal bool Continue { get; }

        internal InvocationOnDispatcher Transform { get; }

        internal ProxyFiberSolver(InvocationOnDispatcher transform, bool @continue)
        {
            Continue = @continue;
            Transform = transform;
        }
    }
}
