using Castle.DynamicProxy;
using EasyActor.Factories;
using EasyActor.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    public class LoadBalancerFactory : ILoadBalancerFactory
    {
        private ActorFactory _ActorFactory;
        private BalancingOption _BalancingOption;

        public LoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Priority priority = Priority.Normal)
        {
            _ActorFactory = new ActorFactory(priority);
            _BalancingOption = option;
        }


        public T Build<T>(Func<T> concrete, int ParrallelLimitation) where T : class
        {
            if (ParrallelLimitation <= 0)
                throw new ArgumentOutOfRangeException("ParrallelLimitation should be positive");

            var interceptors = new IInterceptor[] { new LoadBalanderInterceptor<T>(concrete, _BalancingOption, _ActorFactory, ParrallelLimitation) };
            return ActorFactoryBase.Generator.CreateInterfaceProxyWithoutTarget<T>(interceptors);
        }
    }
}
