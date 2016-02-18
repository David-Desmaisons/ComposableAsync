using System;

namespace EasyActor.Pipeline
{
    public static class ActorBuilder
    {
        private static readonly Lazy<IFactoryBuilder> _ActorFactoryBuilder = new Lazy<IFactoryBuilder>(() => new FactoryBuilder(), true);
        private static readonly Lazy<IActorFactory> _ActorFactory = new Lazy<IActorFactory>(() => _ActorFactoryBuilder.Value.GetFactory(), true);
        private static readonly Lazy<ILoadBalancerFactory> _LoadBalancer = new Lazy<ILoadBalancerFactory>(() => _ActorFactoryBuilder.Value.GetLoadBalancerFactory(), true);

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
