using System;
using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy
{
    internal sealed class FiberProviderInterceptor : InterfaceInterceptor<IFiberProvider>
    {
        private static readonly MethodInfo _FiberMethodInfo = Type.GetProperty(nameof(IFiberProvider.Fiber), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(false);
        private readonly IFiber _Fiber;

        public FiberProviderInterceptor(IFiber fiber)
        {
            _Fiber = fiber;
        }

        protected override object InterceptClassMethod(IInvocation invocation)
        {
            Debug.Assert(invocation.Method == _FiberMethodInfo);
            return _Fiber;
        }
    }
}
