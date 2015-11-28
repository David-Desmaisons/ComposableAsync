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
        public IActorFactory GetFactory(bool Shared = false, Action<Thread> onCreate = null)
        {
            if (Shared)
                return new SharedThreadActorFactory(onCreate);

            return new ActorFactory(onCreate);
        }

        public IActorFactory GetInContextFactory()
        {
            return new SynchronizationContextFactory();
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
