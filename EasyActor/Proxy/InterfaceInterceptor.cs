using Castle.DynamicProxy;
using System;

namespace EasyActor.Proxy
{
    public abstract class InterfaceInterceptor<T> : IInterceptor where T : class
    {
        protected static readonly Type Type = typeof(T);
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (!method.DeclaringType.IsAssignableFrom(Type))
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = InterceptClassMethod(invocation);
        }

        protected abstract object InterceptClassMethod(IInvocation invocation);
    }
}
