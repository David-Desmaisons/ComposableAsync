﻿using System;
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

            return new ProxyFiberSolver(res, res == null || ShouldContinue(method));
        }

        private static bool ShouldContinue(MethodInfo method) 
        {
            return (method == DisposabeInterceptor.DisposeAsync) || (method == DisposabeInterceptor.Dispose);
        }

        private static object Proceed<TResult>(IFiber fiber, IInvocation invocation)
        {
            return fiber.Enqueue(invocation.Call<Task<TResult>>);
        }
    }
}
