using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy 
{
    internal class DispatcherInterceptor<T> : IInterceptor
    {
        private readonly IDispatcher _Dispatcher;

        public DispatcherInterceptor(IDispatcher dispatcher)
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
