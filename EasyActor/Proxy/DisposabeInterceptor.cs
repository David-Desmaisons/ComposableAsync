using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Tasks;

namespace EasyActor.Proxy {
    internal sealed class DisposabeInterceptor : InterfaceInterceptor<IAsyncDisposable> {
        internal static MethodInfo DisposeAsync { get; } = Type.GetMethod(nameof(IAsyncDisposable.DisposeAsync), BindingFlags.Instance | BindingFlags.Public);
        internal static MethodInfo Dispose { get; } = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), BindingFlags.Instance | BindingFlags.Public);

        private readonly IFiber _Fiber;
        private readonly IAsyncDisposable _FiberDisposable;
        private readonly IAsyncDisposable _ActorDisposable;

        public DisposabeInterceptor(IFiber fiber, IAsyncDisposable actorDisposable, IAsyncDisposable fiberDisposable)
        {
            _Fiber = fiber;
            _FiberDisposable = fiberDisposable;
            _ActorDisposable = actorDisposable;
        }

        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method == DisposeAsync) 
            {
                var currentResult = (Task) invocation.ReturnValue;
                return (currentResult ?? ((_ActorDisposable!=null) ? _Fiber.Enqueue(() => _ActorDisposable.DisposeAsync()) : TaskBuilder.Completed)).ContinueWith(_ => _FiberDisposable.DisposeAsync()).Unwrap();
            }

            if (invocation.Method == Dispose) 
            {
                _ActorDisposable?.Dispose();
                _FiberDisposable.Dispose();          
                return null;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
