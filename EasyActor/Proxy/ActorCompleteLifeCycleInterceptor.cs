using System;
using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using EasyActor.Factories;
using EasyActor.Fiber;
using EasyActor.TaskHelper;

namespace EasyActor.Proxy
{
    internal sealed class ActorCompleteLifeCycleInterceptor:  InterfaceInterceptor<IActorCompleteLifeCycle>, IInterceptor
    {
        private static readonly MethodInfo _Abort = Type.GetMethod("Abort", BindingFlags.Instance | BindingFlags.Public);

        private readonly IAbortableFiber _Queue;
        private readonly IAsyncDisposable _AsyncDisposable;

        public ActorCompleteLifeCycleInterceptor(IAbortableFiber queue, IAsyncDisposable asyncDisposable)
        {
            _Queue = queue;
            _AsyncDisposable = asyncDisposable;
        }

        [DebuggerNonUserCode]
        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method != _Abort)
                throw new ArgumentOutOfRangeException();

            return _Queue.Abort(() =>
            {
                ActorFactoryBase.Clean(invocation.Proxy);
                return (_AsyncDisposable != null) ? _AsyncDisposable.DisposeAsync() : TaskBuilder.Completed;
            });
        }
    }
}
