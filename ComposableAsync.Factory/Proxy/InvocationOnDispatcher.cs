using Castle.DynamicProxy;

namespace ComposableAsync.Factory.Proxy
{
    internal delegate object InvocationOnDispatcher(IDispatcher dispatcher, IInvocation invocation);
}
