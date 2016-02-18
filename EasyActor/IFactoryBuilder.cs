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
    public interface IFactoryBuilder
    {
        /// <summary>
        /// Returns an actor factory 
        /// <param name="Shared">
        /// if true all the actors created by this factory will use the same thread. 
        /// </param>
        /// <param name="onCreate">
        /// Delegate called on every new created thread. 
        /// </param>
        /// </summary>
        IActorFactory GetFactory(bool Shared=false, Action<Thread> onCreate = null);

        /// <summary>
        /// Returns an actor factory corresponding to the given ActorFactorType
        /// </summary>
        IActorFactory GetTaskBasedFactory();       
        
        /// <summary>
        /// Returns an actor factory corresponding to the current threading context
        /// </summary>
        IActorFactory GetInContextFactory();

        /// <summary>
        /// Returns an actor factory using the provided synchronizationContext 
        /// </summary>
        IActorFactory GetInContextFactory(SynchronizationContext synchronizationContext);

        /// <summary>
        ///  Returns an load balancer factory corresponding to the given BalancingOption
        /// </summary>
        ILoadBalancerFactory GetLoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Action<Thread> onCreate = null);
    }
}
