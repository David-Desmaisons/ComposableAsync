using System;
using System.Threading;
using Concurrent;
using EasyActor.DispatcherManangers;
using EasyActor.Factories;

namespace EasyActor
{
    public class FactoryBuilder : IFactoryBuilder
    {
        public IActorFactory GetFactory(bool shared = false, Action<Thread> onCreate = null)
        {
            return shared ? new ActorFactory(new SharedThreadFiberManager(onCreate)) : 
                            new ActorFactory(new StandardFiberManager(onCreate));
        }

        public IActorFactory GetThreadPoolFactory()
        {
            return new ActorFactory(new TheadPoolFiberManager());
        }

        public IActorFactory GetInContextFactory()
        {
            return new ActorFactory(new SynchronizationContextFiberManage());
        }

        public IActorFactory GetInContextFactory(SynchronizationContext synchronizationContext)
        {
            return new ActorFactory(new SynchronizationContextFiberManage(synchronizationContext));
        }

        public IActorFactory GetTaskBasedFactory()
        {
            return new ActorFactory(new TaskPoolFiberManager());
        }

        public IActorFactory GetFactoryForFiber(IFiber fiber)
        {
            return new ActorFactory(new DispatcherMananger(fiber));
        }
    }
}
