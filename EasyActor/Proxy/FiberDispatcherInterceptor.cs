using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy 
{
    internal class FiberDispatcherInterceptor<T> : IInterceptor
    {
        private readonly IFiber _Fiber;

        public FiberDispatcherInterceptor(IFiber fiber)
        {
            _Fiber = fiber;
        }

        public void Intercept(IInvocation invocation)
        {
            var proxyFiberSolver = FiberBehaviourCacherDispatcher<T>.GetTransformFunction(invocation);
            invocation.ReturnValue = proxyFiberSolver.Transform?.Invoke(_Fiber, invocation);
            if (proxyFiberSolver.Continue)
            {
                invocation.Proceed();
            }
        }
    }
}
