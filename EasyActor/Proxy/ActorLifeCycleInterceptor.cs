using System;
using System.Reflection;
using Castle.DynamicProxy;
using EasyActor.Disposable;
using EasyActor.Factories;
using EasyActor.Fiber;
using EasyActor.TaskHelper;

namespace EasyActor.Proxy
{
    internal sealed class ActorLifeCycleInterceptor : InterfaceInterceptor<IActorLifeCycle>
    {
        private static readonly MethodInfo _Stop = Type.GetMethod(nameof(IActorLifeCycle.Stop), BindingFlags.Instance | BindingFlags.Public);

        private readonly IStopableFiber _Fiber;
        private readonly IAsyncDisposable _IAsyncDisposable;

        public ActorLifeCycleInterceptor(IStopableFiber fiber, IAsyncDisposable iAsyncDisposable)
        {
            _Fiber = fiber;
            _IAsyncDisposable = iAsyncDisposable;
        }

        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Stop)
                throw new ArgumentOutOfRangeException();

            return _Fiber.Stop(() =>
            {
                ActorFactoryBase.Clean(invocation.Proxy);
                return (_IAsyncDisposable != null) ? _IAsyncDisposable.DisposeAsync() : TaskBuilder.Completed;
            });
        }
    }
}
