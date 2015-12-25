using EasyActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public static class ActorBuilder
    {
        private static Lazy<IFactoryBuilder> _ActorFactoryBuilder = new Lazy<IFactoryBuilder>(() => new ActorFactoryBuilder(), true);
        private static Lazy<IActorFactory> _ActorFactory = new Lazy<IActorFactory>(() => _ActorFactoryBuilder.Value.GetFactory(), true);
        private static Lazy<ILoadBalancerFactory> _LoadBalancer = new Lazy<ILoadBalancerFactory>(() => _ActorFactoryBuilder.Value.GetLoadBalancerFactory(), true);

        public static  IActorFactory ActorFactory
        {
            get { return _ActorFactory.Value; ; }
        }

        public static ILoadBalancerFactory LoadBalancer
        {
            get { return _LoadBalancer.Value; }
        }
    }
}
