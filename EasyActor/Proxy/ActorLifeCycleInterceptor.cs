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
        private IAsyncDisposable _IAsyncDisposable;

        public ActorLifeCycleInterceptor(MonoThreadedQueue iqueue, IAsyncDisposable iAsyncDisposable)
        {
            _Queue = iqueue;
            _IAsyncDisposable = iAsyncDisposable;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (method.DeclaringType != typeof(IActorLifeCycle))
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = _Queue.SetCleanUp(() =>
                (_IAsyncDisposable != null) ? _IAsyncDisposable.DisposeAsync() : TaskBuilder.GetCompleted());

            if (method == _Stop)
            {
                _Queue.Stop();
                return;
            }
            
            if (method == _Abort)
            {
                _Queue.Dispose();
            }
        }
    }


}
