using System;
using FluentAssertions;
 
using System.Threading;
using EasyActor.Factories;
using Xunit;

namespace EasyActor.Test
{
     
    public class ActorFactoryBuilderTest : IDisposable
    {
         private readonly FactoryBuilder _ActorFactoryBuilder;

        public ActorFactoryBuilderTest()
        {
            _ActorFactoryBuilder = new FactoryBuilder();
        }

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        [Fact]
        public void GetFactory_Shared_Return_SharedThreadActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(true);

            res.Should().BeAssignableTo<SharedThreadActorFactory>();
        }

        [Fact]
        public void GetFactory_Standard_Return_ActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(false);

            res.Should().BeOfType<ActorFactory>();
        }

        [Fact]
        public void GetTaskBasedFactory_Return_TaskPoolActorFactory()
        {
            var res = _ActorFactoryBuilder.GetTaskBasedFactory();

            res.Should().BeOfType<TaskPoolActorFactory>();
        }

        [Fact]
        public void GetFactory_InCurrentContext_Return_SynchronizationContextFactory()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var res = _ActorFactoryBuilder.GetInContextFactory();

            res.Should().BeAssignableTo<SynchronizationContextFactory>();
        }

        [Fact]
        public void GetFactory_InCurrentContext_With_Argument_Return_SynchronizationContextFactory()
        {
            var res = _ActorFactoryBuilder.GetInContextFactory(new SynchronizationContext());

            res.Should().BeAssignableTo<SynchronizationContextFactory>();
        }


        [Fact]
        public void GetFactory_LoadBalancerFactory_Return_LoadBalancerFactory()
        {
            var res = _ActorFactoryBuilder.GetLoadBalancerFactory();

            res.Should().BeOfType<LoadBalancerFactory>();
        }
    }
}
