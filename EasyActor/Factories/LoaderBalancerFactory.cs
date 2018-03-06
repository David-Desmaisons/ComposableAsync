using Castle.DynamicProxy;
using EasyActor.Factories;
using EasyActor.Helper;
using EasyActor.Proxy;
using System;
using System.Threading;

namespace EasyActor
{
    public class LoadBalancerFactory : ILoadBalancerFactory
    {
        private readonly ActorFactory _ActorFactory;
        private readonly BalancingOption _BalancingOption;

        public LoadBalancerFactory(BalancingOption option = BalancingOption.MinizeObjectCreation, Action<Thread> onCreate = null)
        {
            _ActorFactory = new ActorFactory(onCreate);
            _BalancingOption = option;
        }


        public T Build<T>(Func<T> concrete, int parrallelLimitation) where T : class
        {
            if (parrallelLimitation <= 0)
                throw new ArgumentOutOfRangeException("ParrallelLimitation should be positive");

            var interceptors = new IInterceptor[] { new LoadBalanderInterceptor<T>(concrete, _BalancingOption, 
                                                        _ActorFactory, parrallelLimitation) };
            return (T)ActorFactoryBase.Generator.CreateInterfaceProxyWithoutTarget(typeof(T), 
                            new [] { TypeHelper.ActorCompleteLifeCycleType }, interceptors);
        }
    }
}
