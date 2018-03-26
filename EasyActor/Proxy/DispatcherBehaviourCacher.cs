using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Tasks;

namespace EasyActor.Proxy
{
    internal static class FiberBehaviourCacherDispatcher<T>
    {
        private static readonly MethodInfo _Proceed = typeof(FiberBehaviourCacherDispatcher<T>).GetMethod(nameof(Proceed), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<MethodInfo, ProxyFiberSolver> _Cache = new Dictionary<MethodInfo, ProxyFiberSolver>();

        static FiberBehaviourCacherDispatcher()
        {
            var type = typeof(T);
            RegisterType(type);
            foreach (var @interface in type.GetInterfaces())
            {
                RegisterType(@interface);
            }
        }

        private static void RegisterType(Type t)
        {
            foreach (var methodInfo in t.GetMethods())
            {
                _Cache.Add(methodInfo, BuildTransformFunction(methodInfo));
            }
        }

        public static ProxyFiberSolver GetTransformFunction(IInvocation invocation)
        {
            ProxyFiberSolver res;
            if (_Cache.TryGetValue(invocation.Method, out res))
                return res;

            return new ProxyFiberSolver(null, true);
        }

        private static ProxyFiberSolver BuildTransformFunction(MethodInfo method)
        {
            Func<IDispatcher, IInvocation, object> res = null;
            var td = method.ReturnType.GetTaskType();
            switch (td.MethodType)
            {
                case TaskType.Void:
                    return new ProxyFiberSolver(Dispatch, false);

                case TaskType.Task:
                    return new ProxyFiberSolver(Enqueue, false);

                case TaskType.GenericTask:
                    var mi = _Proceed.MakeGenericMethod(td.Type);
                    res = (dispatcher, invocation) => mi.Invoke(null, new object[] { dispatcher, invocation });
                    break;

                case TaskType.None:
                    throw new NotSupportedException("Actor method should only return Task, Task<T> or void");
            }

            return new ProxyFiberSolver(res, false);
        }

        private static object Dispatch(IDispatcher dispatcher, IInvocation invocation)
        {
            dispatcher.Dispatch(invocation.Call);
            return null;
        }

        private static object Enqueue(IDispatcher dispatcher, IInvocation invocation) => 
            dispatcher.Enqueue(invocation.Call<Task>);

        private static object Proceed<TResult>(IDispatcher dispatcher, IInvocation invocation) =>
            dispatcher.Enqueue(invocation.Call<Task<TResult>>);
    }
}
