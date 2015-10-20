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
        IActorFactory GetFactory(ActorFactorType type);
    }
}
