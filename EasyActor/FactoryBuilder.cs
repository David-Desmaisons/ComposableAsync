using System;
using System.Threading;

namespace EasyActor
{
    public class FactoryBuilder : IFactoryBuilder
    {
        public IActorFactory GetFactory(bool shared = false, Action<Thread> onCreate = null)
        {
            if (shared)
                return new SharedThreadActorFactory(onCreate);

            return new ActorFactory(onCreate);
        }

        public IActorFactory GetInContextFactory()
        {
            return new SynchronizationContextFactory();
        }

        public IActorFactory GetInContextFactory(SynchronizationContext synchronizationContext)
        {
            return new SynchronizationContextFactory(synchronizationContext);
        }

        public IActorFactory GetTaskBasedFactory()
        {
            return new TaskPoolActorFactory();
        }

        public ILoadBalancerFactory GetLoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Action<Thread> onCreate = null)
        {
            return new LoadBalancerFactory(option, onCreate);
        }
    }
}
