using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.TaskHelper;

namespace EasyActor.Proxy
{
    internal class FiberDispatcherInterceptor<T> : IInterceptor
    {
        private static readonly MethodInfo _Proceed = typeof(FiberDispatcherInterceptor<T>).GetMethod(nameof(Proceed), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Type _Type = typeof(T);

        private readonly IFiber _Fiber;

        public FiberDispatcherInterceptor(IFiber fiber)
        {
            _Fiber = fiber;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            if (!method.DeclaringType.IsAssignableFrom(_Type))
            {
                invocation.Proceed();
                return;
            }

            var td = method.ReturnType.GetTaskType();
            switch (td.MethodType)
            {
                case TaskType.Void:
                    _Fiber.Dispatch(invocation.Call);
                    invocation.ReturnValue = null;
                    break;

                case TaskType.Task:
                    invocation.ReturnValue = _Fiber.Enqueue(invocation.Call<Task>);
                    break;

                case TaskType.GenericTask:
                    var mi = _Proceed.MakeGenericMethod(td.Type);
                    mi.Invoke(this, new object[] { invocation });
                    break;

                case TaskType.None:
                    throw new NotSupportedException("Actor method should only return Task, Task<T> or void");
            }
        }

        private void Proceed<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = _Fiber.Enqueue(invocation.Call<Task<TResult>>);
        }
    }
}
