using System;
using System.Threading;
using Castle.DynamicProxy;
using EasyActor.Helper;
using EasyActor.Proxy;

namespace EasyActor.Factories
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
                throw new ArgumentOutOfRangeException(nameof(parrallelLimitation),"ParrallelLimitation should be positive");

            var interceptors = new IInterceptor[] { new LoadBalanderInterceptor<T>(concrete, _BalancingOption, 
                                                        _ActorFactory, parrallelLimitation) };
            return (T)ActorFactoryBase.Generator.CreateInterfaceProxyWithoutTarget(typeof(T), 
                            new [] { TypeHelper.ActorCompleteLifeCycleType }, interceptors);
        }
    }
}
