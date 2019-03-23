using Concurrent;

namespace EasyActor.Proxy
{
    internal class FiberProvider: IFiberProvider
    {
        internal FiberProvider(IFiber fiber)
        {
            Fiber = fiber;
        }

        public IFiber Fiber { get; }
    }
}
