using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    /// IActorFactory and ILoadBalancerFactory factory
    /// </summary>
    public interface IActorFactoryBuilder
    {
        /// <summary>
        ///  Returns an actor factory corresponding to the given ActorFactorType
        /// </summary>
        IActorFactory GetFactory(ActorFactorType type = ActorFactorType.Standard, Priority priority = Priority.Normal);

        /// <summary>
        ///  Returns an load balancer factory corresponding to the given BalancingOption
        /// </summary>
        ILoadBalancerFactory GetLoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Priority priority = Priority.Normal);
    }
}
