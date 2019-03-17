using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace Concurrent
{
    /// <summary>
    /// <see cref="IDispatcher"/> extension methods provider
    /// </summary>
    public static class DispatcherExtension
    {
        public struct FiberAwaiter : INotifyCompletion
        {
            public bool IsCompleted => false;

            private readonly IDispatcher _Fiber;

            public FiberAwaiter(IDispatcher fiber)
            {
                _Fiber = fiber;
            }

            [SecuritySafeCritical]
            public void OnCompleted(Action continuation)
            {
                _Fiber.Dispatch(continuation);
            }

            public void GetResult() { }
        }

        public struct DispatcherAwaiterProvider
        {
            private readonly FiberAwaiter _Awaiter;
            public DispatcherAwaiterProvider(IDispatcher fiber)
            {
                _Awaiter = new FiberAwaiter(fiber);
            }

            public FiberAwaiter GetAwaiter() => _Awaiter;
        }

        public static DispatcherAwaiterProvider SwitchToContext(this IDispatcher fiber)
        {
            return new DispatcherAwaiterProvider(fiber);
        }
    }
}
