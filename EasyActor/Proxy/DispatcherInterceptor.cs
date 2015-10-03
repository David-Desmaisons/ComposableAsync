using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyActor.TaskHelper;
using System.Reflection;
using EasyActor.Queue;

namespace EasyActor
{
    internal class DispatcherInterceptor : IInterceptor
    {
        private AsyncQueueMonoThreadDispatcher _Queue;
        public DispatcherInterceptor(Priority iPriority)
        {
            _Queue = new AsyncQueueMonoThreadDispatcher(iPriority);
        }

        public void Intercept(IInvocation invocation)
        {
            var method =  invocation.Method;
            var td = method.ReturnType.GetTaskType();

            switch(td.MethodType)
            {
                case MethodType.None:
                    throw new NotSupportedException("Method should be return Task or Task<T>");

                case MethodType.Task:
                    var tcs = new TaskCompletionSource<object>();

                    invocation.ReturnValue = _Queue.Enqueue(() =>
                        {
                            return invocation.Call<Task>();
                        });

                    break;

                case MethodType.GenericTask:
                     var mi = _Proceed.MakeGenericMethod(td.Type);
                     mi.Invoke(this, new[] { invocation });
                    break;
            }
        }

        private static MethodInfo _Proceed = typeof(DispatcherInterceptor).GetMethod("Proceed", BindingFlags.Instance | BindingFlags.NonPublic);


        private void Proceed<T>(IInvocation invocation)
        {
            invocation.ReturnValue = _Queue.Enqueue(() =>
            {
                return invocation.Call<Task<T>>();
            });
        }
    }
}
