using System.Diagnostics;
using Castle.DynamicProxy;

namespace EasyActor.Proxy
{
    [DebuggerNonUserCode]
    internal static class InvocationExtension
    {
        internal static void Call(this IInvocation @this)
        {
            @this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
        }

        internal static T Call<T>(this IInvocation @this)
        {
            return (T)@this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
        }
    }
}
