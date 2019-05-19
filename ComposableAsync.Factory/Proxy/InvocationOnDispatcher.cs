using Castle.DynamicProxy;

namespace ComposableAsync.Factory.Proxy
{
    internal delegate object InvocationOnDispatcher(ICancellableDispatcher dispatcher, IInvocation invocation);
}
