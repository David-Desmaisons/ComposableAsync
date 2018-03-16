using System;
using System.Reflection;
using Castle.DynamicProxy;

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
            if (invocation.Method != _FiberMethodInfo)
                throw new ArgumentOutOfRangeException();

            return _Fiber;
        }
    }
}
