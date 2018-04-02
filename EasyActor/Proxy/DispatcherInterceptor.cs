using System.Diagnostics;
using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy 
{
    [DebuggerNonUserCode]
    internal class DispatcherInterceptor<T> : IInterceptor
    {
        private readonly ICancellableDispatcher _Dispatcher;

        public DispatcherInterceptor(ICancellableDispatcher dispatcher)
        {
            _Dispatcher = dispatcher;
        }

        public void Intercept(IInvocation invocation)
        {
            var proxyFiberSolver = FiberBehaviourCacherDispatcher<T>.GetTransformFunction(invocation);
            if (proxyFiberSolver.Continue) 
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = proxyFiberSolver.Transform.Invoke(_Dispatcher, invocation);
        }
    }
}
