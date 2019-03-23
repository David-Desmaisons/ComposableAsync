using System.Windows.Threading;
using Concurrent.Dispatchers;
using Concurrent.WPF.Internal;
using EasyActor;

namespace Concurrent.WPF
{
    /// <summary>
    /// Extensions on object
    /// </summary>
    public static class FiberExtension
    {
        /// <summary>
        /// Returns the fiber associated with an object
        /// This will returns fiber for both actors and <see cref="DispatcherObject"/>
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static IFiber GetAssociatedFiber(this object @object)
        {
            var fiber = (@object as IFiberProvider)?.Fiber;
            if (fiber != null)
                return fiber;

            return (@object is DispatcherObject dispatch) ? Fiber.GetFiberFromSynchronizationContext(dispatch.Dispatcher.GetSynchronizationContext()) : null;
        }

        /// <summary>
        /// returns dispatcher associated with an object
        /// NullDispatcher if there is no context associated with the object
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static IDispatcher GetAssociatedDispatcher(this object @object)
        {
            return @object.GetAssociatedFiber() ?? NullDispatcher.Instance;
        }
    }
}
