using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    public class ActorFactoryBuilder : IActorFactoryBuilder
    {
        public IActorFactory GetFactory(ActorFactorType type, Priority priority = Priority.Normal)
        {
            switch (type)
            {
                case ActorFactorType.Shared:
                    return new SharedThreadActorFactory(priority);

                case ActorFactorType.Standard:
                    return new ActorFactory(priority);

                case ActorFactorType.InCurrentContext:
                    return new SynchronizationContextFactory();
            }

            throw new ArgumentException("type not supported!");
        }


        public ILoadBalancerFactory GetLoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation)
        {
            return new LoadBalancerFactory(option);
        }
    }
}
