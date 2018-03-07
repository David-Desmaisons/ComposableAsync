using System;
using System.Reflection;
using Castle.DynamicProxy;
using EasyActor.Factories;
using EasyActor.Queue;
using EasyActor.TaskHelper;

namespace EasyActor.Proxy
{
    internal sealed class ActorLifeCycleInterceptor : InterfaceInterceptor<IActorLifeCycle>, IInterceptor
    {
        private static readonly MethodInfo _Stop = Type.GetMethod("Stop", BindingFlags.Instance | BindingFlags.Public);

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
