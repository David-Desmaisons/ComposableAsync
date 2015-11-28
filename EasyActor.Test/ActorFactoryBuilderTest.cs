using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using System.Threading;

namespace EasyActor.Test
{
    [TestFixture]
    public class ActorFactoryBuilderTest
    {
         private ActorFactoryBuilder _ActorFactoryBuilder;

        [SetUp]
        public void TestUp()
        {
            _ActorFactoryBuilder = new ActorFactoryBuilder();
        }

        [TearDown]
        public void TearDown()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        [Test]
        public void GetFactory_Shared_Return_SharedThreadActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(true);

            res.Should().BeAssignableTo<SharedThreadActorFactory>();
        }

        [Test]
        public void GetFactory_Standard_Return_ActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(false);

            res.Should().BeAssignableTo<ActorFactory>();
        }

        [Test]
        public void GetTaskBasedFactory_Return_TaskPoolActorFactory()
        {
            var res = _ActorFactoryBuilder.GetTaskBasedFactory();

            res.Should().BeAssignableTo<TaskPoolActorFactory>();
        }

        [Test]
        public void GetFactory_InCurrentContext_Return_SynchronizationContextFactory()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var res = _ActorFactoryBuilder.GetInContextFactory();

            res.Should().BeAssignableTo<SynchronizationContextFactory>();
        }


        [Test]
        public void GetFactory_LoadBalancerFactory_Return_LoadBalancerFactory()
        {
            var res = _ActorFactoryBuilder.GetLoadBalancerFactory();

            res.Should().BeOfType<LoadBalancerFactory>();
        }
    }
}
