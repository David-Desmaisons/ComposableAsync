using Castle.DynamicProxy;
using ComposableAsync;

namespace EasyActor.Proxy
{
    internal delegate object InvocationOnDispatcher(ICancellableDispatcher dispatcher, IInvocation invocation);
}
