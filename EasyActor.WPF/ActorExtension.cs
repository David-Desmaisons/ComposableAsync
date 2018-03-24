using System.Windows.Threading;
using Concurrent.Dispatchers;
using Concurrent.WPF.Internal;

namespace Concurrent.WPF
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object @object)
        {
            var fiber = EasyActor.ActorExtension.GetAssociatedFiber(@object);
            if (fiber != null)
                return fiber;

            var dispactch = @object as DispatcherObject;
            return (dispactch != null) ? Fiber.GetFiberFromSynchronizationContext(dispactch.Dispatcher.GetSynchronizationContext()) : null;
        }

        public static IDispatcher GetAssociatedDispatcher(this object @object)
        {
            return (IDispatcher)@object.GetAssociatedFiber() ?? NullDispatcher.Instance;
        }
    }
}
