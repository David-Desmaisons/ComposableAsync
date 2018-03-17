using System;
using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Disposable;
using EasyActor.Factories;

namespace EasyActor.Proxy
{
    internal sealed class ActorCompleteLifeCycleInterceptor : InterfaceInterceptor<IActorCompleteLifeCycle>
    {
        private static readonly MethodInfo _Abort = Type.GetMethod(nameof(IActorCompleteLifeCycle.Abort), BindingFlags.Instance | BindingFlags.Public);

        private readonly IAbortableFiber _Fiber;
        private readonly IAsyncDisposable _AsyncDisposable;

        public ActorCompleteLifeCycleInterceptor(IAbortableFiber fiber, IAsyncDisposable asyncDisposable)
        {
            _Fiber = fiber;
            _AsyncDisposable = asyncDisposable;
        }

        [DebuggerNonUserCode]
        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Abort)
                throw new ArgumentOutOfRangeException();

            return _Fiber.Abort(() =>
            {
                ActorFactoryBase.Clean(invocation.Proxy);
                return _AsyncDisposable?.DisposeAsync() ?? TaskBuilder.Completed;
            });
        }
    }
}
