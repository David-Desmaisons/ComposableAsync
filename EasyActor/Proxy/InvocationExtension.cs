using System;
using Castle.DynamicProxy;
using System.Diagnostics;
using System.Reflection;

namespace EasyActor.Proxy
{
    [DebuggerNonUserCode]
    internal static class InvocationExtension
    {
        internal static void Call(this IInvocation @this)
        {
            try
            {
                @this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal static T Call<T>(this IInvocation @this)
        {
            try
            {
                return (T)@this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
            }
            catch (TargetInvocationException targetException)
            {
                throw targetException.InnerException ?? targetException;
            }
        }
    }
}
