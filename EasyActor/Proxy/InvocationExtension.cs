using Castle.DynamicProxy;

namespace EasyActor.Proxy
{
    public static class InvocationExtension
    {
        public static void Call(this IInvocation @this)
        {
            @this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
        }

        public static T Call<T>(this IInvocation @this)
        {
            return (T)@this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
        }
    }
}
