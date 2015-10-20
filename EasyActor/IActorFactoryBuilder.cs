using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///   IActorFactory factory
    /// </summary>
    public interface IActorFactoryBuilder
    {
        /// <summary>
        ///  Returns an actor factory corresponding to the ActorFactorType
        /// </summary>
        IActorFactory GetFactory(ActorFactorType type, Priority priority = Priority.Normal);
    }
}
