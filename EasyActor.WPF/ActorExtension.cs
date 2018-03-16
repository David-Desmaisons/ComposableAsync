using System.Windows.Threading;
using EasyActor.WPF.Fiber;

namespace EasyActor.WPF
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object rawimplementation)
        {
            var fiber = EasyActor.ActorExtension.GetAssociatedFiber(rawimplementation);
            if (fiber != null)
                return fiber;

            var dispactch = rawimplementation as DispatcherObject;
            return (dispactch != null) ? new DispatcherFiber(dispactch) : null;
        }
    }
}
