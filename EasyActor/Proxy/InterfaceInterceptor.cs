using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Proxy
{
    public abstract class InterfaceInterceptor<T> : IInterceptor where T : class
    {
        protected static Type _Type = typeof(T);
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (method.DeclaringType != _Type)
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = InterceptClassMethod(invocation);
        }

        protected abstract object InterceptClassMethod(IInvocation invocation);
    }
}
