using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using Concurrent.Tasks;

namespace EasyActor.Proxy
{
    [DebuggerNonUserCode]
    internal static class FiberBehaviourCacherDispatcher<T>
    {
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
            var registeredMethodInfo = invocation.Method.IsGenericMethod ? invocation.Method.GetGenericMethodDefinition() : invocation.Method;
            return _Cache.TryGetValue(registeredMethodInfo, out var res) ? res : new ProxyFiberSolver(null, true);
        }

        private static ProxyFiberSolver BuildTransformFunction(MethodInfo method)
        {
            var td = method.ReturnType.GetTaskType();
            switch (td.MethodType)
            {
                case TaskType.Void:
                    return new ProxyFiberSolver(DispatcherBehaviour.DispatchFunction, false);

                case TaskType.Task:
                    var invocationOnDispatcher = DispatcherBehaviour.GetInvocationOnDispatcherForTask(method);
                    return new ProxyFiberSolver(invocationOnDispatcher, false);

                case TaskType.GenericTask:
                    if (td.Type.IsGenericParameter)
                    {
                        var dynamicInvocationOnDispatcher = DispatcherBehaviour.GetDynamicInvocationOnDispatcher(method);
                        return new ProxyFiberSolver(dynamicInvocationOnDispatcher, false);
                    }                     

                    var function = DispatcherBehaviour.GetInvocationOnDispatcherForGenericTask(td.Type, method);
                    return new ProxyFiberSolver(function, false);
            }

            throw new NotSupportedException("Actor method should only return Task, Task<T> or void");
        }
    }
}
