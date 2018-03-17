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
            var function = FiberBehaviourCacherDispatcher<T>.GetTransformFunction(invocation);
            if (function==null)
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = function(_Fiber, invocation);
        }
    }
}
