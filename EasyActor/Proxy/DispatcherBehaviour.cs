using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Tasks;

namespace EasyActor.Proxy
{
    public static class DispatcherBehaviour
    {
        private static readonly MethodInfo _Proceed = typeof(DispatcherBehaviour).GetMethod(nameof(Proceed), BindingFlags.Static | BindingFlags.NonPublic);

        internal static readonly InvocationOnDispatcher DynamicEnqueueFunction = DynamicEnqueue;
        internal static readonly InvocationOnDispatcher EnqueueFunction = Enqueue;
        internal static readonly InvocationOnDispatcher DispatchFunction = Dispatch;

        internal static InvocationOnDispatcher BuildDynamic(Type targetType)
        {
            var methodInfo = _Proceed.MakeGenericMethod(targetType);
            return (InvocationOnDispatcher)Delegate.CreateDelegate(typeof(InvocationOnDispatcher), methodInfo);
        }

        private static object Dispatch(IDispatcher dispatcher, IInvocation invocation)
        {
            dispatcher.Dispatch(invocation.Call);
            return null;
        }

        private static object DynamicEnqueue(IDispatcher dispatcher, IInvocation invocation)
        {
            var solvedTask = invocation.Method.ReturnType.GetTaskType();
            var methodInfo = _Proceed.MakeGenericMethod(solvedTask.Type);
            return methodInfo.Invoke(null, new object[] { dispatcher, invocation });
        }

        private static object Enqueue(IDispatcher dispatcher, IInvocation invocation) =>
            dispatcher.Enqueue(invocation.Call<Task>);

        private static object Proceed<TResult>(IDispatcher dispatcher, IInvocation invocation) =>
            dispatcher.Enqueue(invocation.Call<Task<TResult>>);
    }
}
