using System;
using System.Threading.Tasks;
using System.Reflection;
using Castle.DynamicProxy;

using EasyActor.TaskHelper;

namespace EasyActor
{
    internal class QueueDispatcherInterceptor : IInterceptor
    {
        private static readonly MethodInfo _Proceed = typeof(QueueDispatcherInterceptor).GetMethod(nameof(Proceed), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ITaskQueue _Queue;

        public QueueDispatcherInterceptor(ITaskQueue iqueue)
        {
            _Queue = iqueue;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            var td = method.ReturnType.GetTaskType();

            switch (td.MethodType)
            {
                case TaskType.None:
                    throw new NotSupportedException("Actor method should only return Task or Task<T>");

                case TaskType.Task:
                    invocation.ReturnValue = _Queue.Enqueue(() =>
                        {
                            return invocation.Call<Task>();
                        });

                    break;

                case TaskType.GenericTask:
                    var mi = _Proceed.MakeGenericMethod(td.Type);
                    mi.Invoke(this, new[] { invocation });
                    break;
            }
        }

        private void Proceed<T>(IInvocation invocation)
        {
            invocation.ReturnValue = _Queue.Enqueue(() =>
            {
                return invocation.Call<Task<T>>();
            });
        }
    }
}
