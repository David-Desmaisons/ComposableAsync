using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    /// IActorFactory and ILoadBalancerFactory factory
    /// </summary>
    public interface IActorFactoryBuilder
    {
        /// <summary>
        /// Returns an actor factory corresponding to the given ActorFactorType
        /// <param name="onCreate">
        /// Delegate called on every new created thread. 
        /// </param>
        /// </summary>
        IActorFactory GetFactory(ActorFactorType type = ActorFactorType.Standard, Action<Thread> onCreate = null);

        /// <summary>
        ///  Returns an load balancer factory corresponding to the given BalancingOption
        /// </summary>
        ILoadBalancerFactory GetLoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Action<Thread> onCreate = null);
    }
}
