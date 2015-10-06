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
        private static MethodInfo _Proceed = typeof(DispatcherInterceptor).GetMethod("Proceed", BindingFlags.Instance | BindingFlags.NonPublic);
       
        private static MethodInfo _Dispose = typeof(IDisposable).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public);


        private MonoThreadedQueue _Queue;
        private bool _Shared;
        public DispatcherInterceptor(MonoThreadedQueue iqueue, bool iShared)
        {
            _Queue = iqueue;
            _Shared = iShared;
        }

        public void Intercept(IInvocation invocation)
        {

            var method =  invocation.Method;

            if (method ==_Dispose)
            {
                if (invocation.MethodInvocationTarget!=null)
                {
                    _Queue.Enqueue(() =>
                    {
                        invocation.Call<object>();
                    });

                    if (!_Shared)
                    {
                         _Queue.Enqueue(() => 
                        {                 
                            _Queue.Dispose();
                        });
                    }
                }

                return;
            }


            var td = method.ReturnType.GetTaskType();

            switch(td.MethodType)
            {
                case TaskType.None:
                    throw new NotSupportedException("Method should be return Task or Task<T>");

                case TaskType.Task:
                    var tcs = new TaskCompletionSource<object>();

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
