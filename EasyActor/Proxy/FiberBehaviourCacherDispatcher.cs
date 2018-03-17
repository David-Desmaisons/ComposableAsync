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
        private static readonly Dictionary<MethodInfo, Func<IFiber, IInvocation, object>> _Cache = new Dictionary<MethodInfo, Func<IFiber, IInvocation, object>>();

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

        public static Func<IFiber, IInvocation, object> GetTransformFunction(IInvocation invocation)
        {
            Func<IFiber, IInvocation, object> res = null;
            _Cache.TryGetValue(invocation.Method, out res);
            return res;
        }

        private static Func<IFiber, IInvocation, object> BuildTransformFunction(MethodInfo method)
        {
            Func<IFiber, IInvocation, object> res = null;
            var td = method.ReturnType.GetTaskType();
            switch (td.MethodType)
            {
                case TaskType.Void:
                    res = (fiber, invocation) =>
                    {
                        fiber.Dispatch(invocation.Call);
                        return null;
                    };
                    break;

                case TaskType.Task:
                    res = (fiber, invocation) => fiber.Enqueue(invocation.Call<Task>);
                    break;

                case TaskType.GenericTask:
                    var mi = _Proceed.MakeGenericMethod(td.Type);
                    res = (fiber, invocation) => mi.Invoke(null, new object[] { fiber, invocation });
                    break;

                case TaskType.None:
                    throw new NotSupportedException("Actor method should only return Task, Task<T> or void");
            }

            return res;
        }

        private static object Proceed<TResult>(IFiber fiber, IInvocation invocation)
        {
            return fiber.Enqueue(invocation.Call<Task<TResult>>);
        }
    }
}
