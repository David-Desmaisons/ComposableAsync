using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent.Tasks;

namespace EasyActor.Proxy
{
    internal sealed class DisposabeInterceptor : InterfaceInterceptor<IAsyncDisposable>
    {
        private static readonly MethodInfo _DisposeAsync = Type.GetMethod(nameof(IAsyncDisposable.DisposeAsync), BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo _Dispose = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), BindingFlags.Instance | BindingFlags.Public);

        private readonly IAsyncDisposable _Disposable;
        private readonly IAsyncDisposable _ActorDisposable;

        public DisposabeInterceptor(IAsyncDisposable actorDisposable, IAsyncDisposable disposable)
        {
            _Disposable = disposable;
            _ActorDisposable = actorDisposable;
        }

        protected override object InterceptClassMethod(IInvocation invocation)
        {
            if (invocation.Method == _DisposeAsync)
            {
                return (_Disposable?.DisposeAsync() ?? TaskBuilder.Completed).ContinueWith(_ => _ActorDisposable.DisposeAsync()).Unwrap();
            }

            if (invocation.Method == _Dispose)
            {
                _Disposable?.Dispose();
                _ActorDisposable.Dispose();
                return null;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
