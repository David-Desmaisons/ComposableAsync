using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public class ActorFactoryBuilder : IActorFactoryBuilder
    {
        public IActorFactory GetFactory(ActorFactorType type, Action<Thread> onCreate = null)
        {
            switch (type)
            {
                case ActorFactorType.Shared:
                    return new SharedThreadActorFactory(onCreate);

                case ActorFactorType.Standard:
                    return new ActorFactory(onCreate);

                case ActorFactorType.InCurrentContext:
                    return new SynchronizationContextFactory();
            }

            throw new ArgumentException("type not supported!");
        }


        public ILoadBalancerFactory GetLoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Action<Thread> onCreate = null)
        {
            return new LoadBalancerFactory(option, onCreate);
        }
    }
}
