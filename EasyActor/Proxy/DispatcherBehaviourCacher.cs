using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using Concurrent.Tasks;

namespace EasyActor.Proxy
{
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
            ProxyFiberSolver res;
            var registeredMethodInfo = invocation.Method.IsGenericMethod ? invocation.Method.GetGenericMethodDefinition() : invocation.Method;
            if (_Cache.TryGetValue(registeredMethodInfo, out res))
                return res;

            return new ProxyFiberSolver(null, true);
        }

        private static ProxyFiberSolver BuildTransformFunction(MethodInfo method)
        {
            var td = method.ReturnType.GetTaskType();
            switch (td.MethodType)
            {
                case TaskType.Void:
                    return new ProxyFiberSolver(DispatcherBehaviour.DispatchFunction, false);

                case TaskType.Task:
                    return new ProxyFiberSolver(DispatcherBehaviour.EnqueueFunction, false);

                case TaskType.GenericTask:
                    if (td.Type.IsGenericParameter)
                        return new ProxyFiberSolver(DispatcherBehaviour.DynamicEnqueueFunction, false);

                    var function = DispatcherBehaviour.BuildDynamic(td.Type);
                    return new ProxyFiberSolver(function, false);
            }

            throw new NotSupportedException("Actor method should only return Task, Task<T> or void");
        }
    }
}
