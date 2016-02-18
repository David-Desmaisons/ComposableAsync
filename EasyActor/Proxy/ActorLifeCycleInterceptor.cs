using Castle.DynamicProxy;
using System;
using EasyActor.TaskHelper;
using System.Reflection;
using EasyActor.Proxy;
using EasyActor.Factories;

namespace EasyActor
{
    internal class ActorLifeCycleInterceptor : InterfaceInterceptor<IActorLifeCycle>, IInterceptor
    {
        private static readonly MethodInfo _Stop = _Type.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public);

        private readonly IStopableTaskQueue _Queue;
        private readonly IAsyncDisposable _IAsyncDisposable;

        public ActorLifeCycleInterceptor(IStopableTaskQueue iqueue, IAsyncDisposable iAsyncDisposable)
        {
            _Queue = iqueue;
            _IAsyncDisposable = iAsyncDisposable;
        }

        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Stop)
                throw new ArgumentOutOfRangeException();

            return _Queue.Stop(() =>
            {
                ActorFactoryBase.Clean(invocation.Proxy);
                return (_IAsyncDisposable != null) ? _IAsyncDisposable.DisposeAsync() : TaskBuilder.Completed;
            });
        }
    }
}
