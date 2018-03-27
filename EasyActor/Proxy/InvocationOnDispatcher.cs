using Castle.DynamicProxy;
using Concurrent;

namespace EasyActor.Proxy
{
    internal delegate object InvocationOnDispatcher(IDispatcher dispatcher, IInvocation invocation);
}
