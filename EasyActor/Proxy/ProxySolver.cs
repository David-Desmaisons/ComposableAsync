using System;
using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy 
{
    internal struct ProxyFiberSolver 
    {
        public bool Continue { get; }

        public Func<IFiber, IInvocation, object> Transform { get; }

        public ProxyFiberSolver(Func<IFiber, IInvocation, object> transform, bool @continue)
        {
            Continue = @continue;
            Transform = transform;
        }
    }
}
