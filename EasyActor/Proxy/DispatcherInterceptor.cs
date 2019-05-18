using System.Diagnostics;
using Castle.DynamicProxy;
using ComposableAsync;

namespace EasyActor.Proxy 
{
    [DebuggerNonUserCode]
    internal class DispatcherInterceptor<T> : IInterceptor
    {
        private readonly ICancellableDispatcher _Dispatcher;

        internal DispatcherInterceptor(ICancellableDispatcher dispatcher)
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

            invocation.ReturnValue = proxyFiberSolver.Invoke(_Dispatcher, invocation);
        }
    }
}
