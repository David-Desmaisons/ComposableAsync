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
using EasyActor.Proxy;

namespace EasyActor
{
    internal class ActorCompleteLifeCycleInterceptor:  InterfaceInterceptor<IActorCompleteLifeCycle>, IInterceptor
    {
         private static MethodInfo _Abort = _Type.GetMethod("Abort", BindingFlags.Instance | BindingFlags.Public);

        private IAbortableTaskQueue _Queue;
        private IAsyncDisposable _IAsyncDisposable;

        public ActorCompleteLifeCycleInterceptor(IAbortableTaskQueue iqueue, IAsyncDisposable iAsyncDisposable)
        {
            _Queue = iqueue;
            _IAsyncDisposable = iAsyncDisposable;
        }

        [DebuggerNonUserCode]
        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Abort)
            {
                throw new ArgumentOutOfRangeException();
            }
           
            return _Queue.Abort(() => (_IAsyncDisposable != null) ? 
                            _IAsyncDisposable.DisposeAsync() : TaskBuilder.Completed);
        }
    }
}
