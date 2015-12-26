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
    internal class ActorLifeCycleInterceptor : InterfaceInterceptor<IActorLifeCycle>, IInterceptor
    {
        private static MethodInfo _Stop = _Type.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public);

        private IStopableTaskQueue _Queue;
        private IAsyncDisposable _IAsyncDisposable;

        public ActorLifeCycleInterceptor(IStopableTaskQueue iqueue, IAsyncDisposable iAsyncDisposable)
        {
            _Queue = iqueue;
            _IAsyncDisposable = iAsyncDisposable;
        }

        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Stop)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _Queue.Stop(() => (_IAsyncDisposable != null) ?
                            _IAsyncDisposable.DisposeAsync() : TaskBuilder.Completed);
        }
    }
}
