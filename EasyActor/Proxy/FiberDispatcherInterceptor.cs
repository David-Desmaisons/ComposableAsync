﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.Fiber;
using EasyActor.TaskHelper;

namespace EasyActor.Proxy
{
    internal class FiberDispatcherInterceptor : IInterceptor
    {
        private static readonly MethodInfo _Proceed = typeof(FiberDispatcherInterceptor).GetMethod(nameof(Proceed), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IFiber _Fiber;

        public FiberDispatcherInterceptor(IFiber fiber)
        {
            _Fiber = fiber;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            var td = method.ReturnType.GetTaskType();

            switch (td.MethodType)
            {
                case TaskType.None:
                    throw new NotSupportedException("Actor method should only return Task, Task<T> or void");

                case TaskType.Void:
                    _Fiber.Dispatch(invocation.Call);
                    break;

                case TaskType.Task:
                    invocation.ReturnValue = _Fiber.Enqueue(invocation.Call<Task>);
                    break;

                case TaskType.GenericTask:
                    var mi = _Proceed.MakeGenericMethod(td.Type);
                    mi.Invoke(this, new object[] { invocation });
                    break;
            }
        }

        private void Proceed<T>(IInvocation invocation)
        {
            invocation.ReturnValue = _Fiber.Enqueue(invocation.Call<Task<T>>);
        }
    }
}
