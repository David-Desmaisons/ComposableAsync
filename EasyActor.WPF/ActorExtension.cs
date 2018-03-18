using System.Windows.Threading;
using Concurrent.WPF.Internal;

namespace Concurrent.WPF
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object rawimplementation)
        {
            var fiber = EasyActor.ActorExtension.GetAssociatedFiber(rawimplementation);
            if (fiber != null)
                return fiber;

            var dispactch = rawimplementation as DispatcherObject;
            return (dispactch != null) ? Fiber.GetFiberFromSynchronizationContext(dispactch.Dispatcher.GetSynchronizationContext()) : null;
        }
    }
}
