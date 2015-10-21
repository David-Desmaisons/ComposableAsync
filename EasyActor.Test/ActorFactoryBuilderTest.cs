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
        [ExpectedException(typeof(ArgumentException))]
        public void GetFactory_InvalidEnum_Throw_Exception()
        {
            var res = _ActorFactoryBuilder.GetFactory((ActorFactorType)10);
        }

        [Test]
        public void GetFactory_Shared_Return_SharedThreadActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(ActorFactorType.Shared);

            res.Should().BeAssignableTo<SharedThreadActorFactory>();
        }

        [Test]
        public void GetFactory_Standard_Return_ActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(ActorFactorType.Standard);

            res.Should().BeAssignableTo<ActorFactory>();
        }

        [Test]
        public void GetFactory_InCurrentContext_Return_SynchronizationContextFactory()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var res = _ActorFactoryBuilder.GetFactory(ActorFactorType.InCurrentContext);

            res.Should().BeAssignableTo<SynchronizationContextFactory>();
        }
    }
}
