using System.Windows.Threading;
using Concurrent.Dispatchers;
using Concurrent.WPF.Internal;
using EasyActor;

namespace Concurrent.WPF
{
    public static class ActorExtension
    {
        public static IFiber GetAssociatedFiber(this object @object)
        {
            var fiber = (@object as IFiberProvider)?.Fiber;
            if (fiber != null)
                return fiber;

            return (@object is DispatcherObject dispatch) ? Fiber.GetFiberFromSynchronizationContext(dispatch.Dispatcher.GetSynchronizationContext()) : null;
        }

        public static IDispatcher GetAssociatedDispatcher(this object @object)
        {
            return @object.GetAssociatedFiber() ?? NullDispatcher.Instance;
        }
    }
}
