using Concurrent;

namespace EasyActor.Proxy
{
    internal class FiberProvider: IFiberProvider
    {
        public FiberProvider(IFiber fiber)
        {
            Fiber = fiber;
        }

        public IFiber Fiber { get; }
    }
}
