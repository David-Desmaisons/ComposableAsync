using System;
using FluentAssertions;
using System.Threading;
using Xunit;
using System.Threading.Tasks;
using EasyActor.Options;

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
        public async Task GetFactory_Shared_Return_SharedThreadActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(true);

            res.Type.Should().Be(ActorFactorType.Shared);
            await res.DisposeAsync();
        }

        [Fact]
        public void GetFactory_Standard_Return_ActorFactory()
        {
            var res = _ActorFactoryBuilder.GetFactory(false);

            res.Type.Should().Be(ActorFactorType.Standard);
        }

        [Fact]
        public void GetTaskBasedFactory_Return_TaskPoolActorFactory()
        {
            var res = _ActorFactoryBuilder.GetTaskBasedFactory();

            res.Type.Should().Be(ActorFactorType.TaskPool);
        }

        [Fact]
        public void GetFactory_InCurrentContext_Return_SynchronizationContextFactory()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var res = _ActorFactoryBuilder.GetInContextFactory();

            res.Type.Should().Be(ActorFactorType.InCurrentContext);
        }

        [Fact]
        public void GetFactory_InCurrentContext_With_Argument_Return_SynchronizationContextFactory()
        {
            var res = _ActorFactoryBuilder.GetInContextFactory(new SynchronizationContext());

            res.Type.Should().Be(ActorFactorType.InCurrentContext);
        }
    }
}
