using System.Windows.Threading;
using Concurrent.Fibers;
using Concurrent.WPF.Fiber;

namespace Concurrent.WPF
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object rawimplementation)
        {
            var fiber = EasyActor.ActorExtension.GetAssociatedFiber(rawimplementation);
            if (fiber != null)
                return fiber;

            return null;
            //var dispactch = rawimplementation as DispatcherObject;
            //return (dispactch != null) ? new DispatcherFiber(dispactch) : null;
        }
    }
}
