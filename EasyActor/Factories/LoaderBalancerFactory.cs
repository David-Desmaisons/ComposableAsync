using Castle.DynamicProxy;
using EasyActor.Factories;
using EasyActor.Helper;
using EasyActor.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public class LoadBalancerFactory : ILoadBalancerFactory
    {
        private ActorFactory _ActorFactory;
        private BalancingOption _BalancingOption;

        public LoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Action<Thread> onCreate = null)
        {
            _ActorFactory = new ActorFactory(onCreate);
            _BalancingOption = option;
        }


        public T Build<T>(Func<T> concrete, int ParrallelLimitation) where T : class
        {
            if (ParrallelLimitation <= 0)
                throw new ArgumentOutOfRangeException("ParrallelLimitation should be positive");

            var interceptors = new IInterceptor[] { new LoadBalanderInterceptor<T>(concrete, _BalancingOption, 
                                                        _ActorFactory, ParrallelLimitation) };
            return (T)ActorFactoryBase.Generator.CreateInterfaceProxyWithoutTarget(typeof(T), 
                            new Type[] { TypeHelper.IActorCompleteLifeCycleType }, interceptors);
        }
    }
}
