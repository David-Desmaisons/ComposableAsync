using Concurrent;

namespace EasyActor
{
    /// <summary>
    /// Returns the fiber associated with an actor
    /// </summary>
    public interface IFiberProvider
    {
        /// <summary>
        /// Returns the corresponding <see cref="IFiber"/>
        /// </summary>
        IFiber Fiber { get; }
    }
}
