using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyActor.TaskHelper;
using System.Reflection;
using EasyActor.Queue;
using System.Diagnostics;

namespace EasyActor
{
    internal class ActorLifeCycleInterceptor : IInterceptor
    {
        private static Type _IActorLifeCycleType = typeof(IActorLifeCycle);
        private static MethodInfo _Abort = _IActorLifeCycleType.GetMethod("Abort", BindingFlags.Instance | BindingFlags.Public);
        private static MethodInfo _Stop = _IActorLifeCycleType.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public);

        private MonoThreadedQueue _Queue;
        private IAsyncDisposable _IAsyncDisposable;

        public ActorLifeCycleInterceptor(MonoThreadedQueue iqueue, IAsyncDisposable iAsyncDisposable)
        {
            _Queue = iqueue;
            _IAsyncDisposable = iAsyncDisposable;
        }

        [DebuggerNonUserCode]
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (method.DeclaringType != _IActorLifeCycleType)
            {
                invocation.Proceed();
                return;
            }

            invocation.ReturnValue = _Queue.SetCleanUp(() =>
                (_IAsyncDisposable != null) ? _IAsyncDisposable.DisposeAsync() : TaskBuilder.Completed);

            if (method == _Stop)
            {
                _Queue.Stop();
            } 
            else if (method == _Abort)
            {
                _Queue.Dispose();
            }
        }
    }


}
