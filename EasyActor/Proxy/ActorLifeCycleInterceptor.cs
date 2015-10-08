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

            if (method == _Stop)
            {
                invocation.ReturnValue = (_IAsyncDisposable != null) ?
                    _Queue.Enqueue(() => { return _IAsyncDisposable.DisposeAsync(); }) : TaskBuilder.GetCompleted();
                _Queue.Stop();
                return;
            }
            
            if (method == _Abort)
            {
                if (_IAsyncDisposable!=null)
                    _Queue.SetCleanUp(() => _IAsyncDisposable.DisposeAsync());

                invocation.ReturnValue =
                  _Queue.SetCleanUp(() =>   (_IAsyncDisposable != null) ? _IAsyncDisposable.DisposeAsync() : TaskBuilder.GetCompleted() ) ;

                _Queue.Dispose();
            }
        }


    }
}
