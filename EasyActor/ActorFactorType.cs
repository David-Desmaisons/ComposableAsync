using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///  Returns the type of the factory.
    /// </summary>
    public enum ActorFactorType
    {
        /// <summary>
        /// Standard factory, all created actors have a dedicated thread.
        /// </summary>
        Standard,

        /// <summary>
        /// Shared factory, all created actors sahre teh same thread.
        /// </summary>
        Shared,

        /// <summary>
        /// In Context factory: actors will use the current SynchronizationContext
        /// such as WPF or windows form thread.
        /// </summary>
        InCurrentContext
    }
}
