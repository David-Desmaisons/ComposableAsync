using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyActor
{
    /// <summary>
    ///  Describe the strategy used by the <see cref="ILoadBalancerFactory"/> to create actors
    /// </summary>
    public enum BalancingOption
    {
        /// <summary>
        /// <see cref="ILoadBalancerFactory"/> prefers actor creation to maximize parrelelism
        /// </summary>
        PreferParralelism,

        /// <summary>
        /// <see cref="ILoadBalancerFactory"/> minizes object creations
        /// </summary>
        MinizeObjectCreation
    }
}