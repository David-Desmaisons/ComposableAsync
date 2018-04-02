using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy
{
    internal delegate object InvocationOnDispatcher(ICancellableDispatcher dispatcher, IInvocation invocation);
}
