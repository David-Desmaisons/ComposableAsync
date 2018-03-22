using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace Concurrent
{
    public static class FiberExtension
    {
        public struct FiberAwaiter : INotifyCompletion
        {
            public bool IsCompleted => false;

            private readonly IFiber _Fiber;

            public FiberAwaiter(IFiber fiber)
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

        public struct FiberAwaiterProvider
        {
            private readonly FiberAwaiter _Awaiter;
            public FiberAwaiterProvider(IFiber fiber)
            {
                _Awaiter = new FiberAwaiter(fiber);
            }

            public FiberAwaiter GetAwaiter() => _Awaiter;
        }

        public static FiberAwaiterProvider SwitchToContext(this IFiber fiber)
        {
            return new FiberAwaiterProvider(fiber);
        }
    }
}
