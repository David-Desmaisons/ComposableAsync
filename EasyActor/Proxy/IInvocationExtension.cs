using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace EasyActor
{
    public static class IInvocationExtension
    {
        public static T Call<T>(this IInvocation @this)
        {
            return (T)@this.Method.Invoke(@this.InvocationTarget, @this.Arguments);
        }
    }
}
