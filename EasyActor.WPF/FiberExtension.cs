using System.Windows.Threading;
using ComposableAsync;
using Concurrent.WPF.Internal;

namespace Concurrent.WPF
{
    /// <summary>
    /// Extensions on object
    /// </summary>
    public static class FiberExtension
    {
        private static ICancellableDispatcher GetRawDispatcher(this object @object)
        {
           return (@object as ICancellableDispatcherProvider)?.Dispatcher;
        }

        /// <summary>
        /// Returns the fiber associated with an object
        /// This will returns fiber for both actors and <see cref="DispatcherObject"/>
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static IFiber GetAssociatedFiber(this object @object)
        {
            var dispatcher = @object.GetRawDispatcher();
            if (dispatcher is IFiber fiber)
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
            return @object.GetRawDispatcher() ?? NullDispatcher.Instance;
        }
    }
}
