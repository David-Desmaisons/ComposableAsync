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
    internal class ActorLifeCycleInterceptor : IInterceptor
    {

        private static MethodInfo _Abort = typeof(IActorLifeCycle).GetMethod("Abort", BindingFlags.Instance | BindingFlags.Public);
        private static MethodInfo _Stop = typeof(IActorLifeCycle).GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public);

        private MonoThreadedQueue _Queue;

        public ActorLifeCycleInterceptor(MonoThreadedQueue iqueue)
        {
            _Queue = iqueue;
        }

        public void Intercept(IInvocation invocation)
        {

            var method = invocation.Method;

            if (method.DeclaringType != typeof(IActorLifeCycle))
            {
                invocation.Proceed();
                return;
            }

            if (invocation.MethodInvocationTarget != null)
            {
                invocation.ReturnValue = _Queue.Enqueue(() =>
                {
                    invocation.Call<Task>();
                });
            }
          
            _Queue.Stop();
        }


    }
}
