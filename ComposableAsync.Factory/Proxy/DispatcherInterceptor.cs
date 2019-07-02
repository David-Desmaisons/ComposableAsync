using System.Diagnostics;
using Castle.DynamicProxy;

namespace ComposableAsync.Factory.Proxy 
{
    [DebuggerNonUserCode]
    internal class DispatcherInterceptor<T> : IInterceptor
    {
        private readonly IDispatcher _Dispatcher;

        internal DispatcherInterceptor(IDispatcher dispatcher)
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
