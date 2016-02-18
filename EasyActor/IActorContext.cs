using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///  Interface to access TaskFactory associated with a specific proxy
    /// </summary>
    public interface IActorContext
    {
        /// <summary>
        /// Returns TaskFactory associated with corresponding proxified object. 
        /// </summary>        
        /// <param name="proxy">
        /// Proxy context.
        /// </param>        
        /// <returns>
        /// A TaskFactory corresponding to proxy.
        /// </returns>
        TaskFactory GetTaskFactory(object proxy);


        /// <summary>
        /// Returns TaskScheduler associated with corresponding proxified object. 
        /// </summary>        
        /// <param name="proxy">
        /// Proxy context.
        /// </param>        
        /// <returns>
        /// The TaskScheduler corresponding to proxy.
        /// </returns>
        TaskScheduler GetTaskScheduler(object proxy);
    }
}
