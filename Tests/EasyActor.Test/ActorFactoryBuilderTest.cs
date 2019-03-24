using System;
using FluentAssertions;
using System.Threading;
using Xunit;
using System.Threading.Tasks;
using Concurrent;
using EasyActor.Test.TestInfra.DummyClass;

namespace EasyActor.Test
{

    public class ActorFactoryBuilderTest : IDisposable
    {
        private readonly ActorFactoryBuilder _ActorFactoryBuilder;

        public ActorFactoryBuilderTest()
        {
            _ActorFactoryBuilder = new ActorFactoryBuilder();
        }

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        [Fact]
        public async Task GetFactoryForFiber_Creates_Actor_Using_The_Provided_Fiber()
        {
            var fiber = Fiber.CreateMonoThreadedFiber();
            var fiberThread = await fiber.Enqueue(() => Thread.CurrentThread);

            var factory = _ActorFactoryBuilder.GetFactoryForFiber(fiber);

            var target = new DummyClass();
            var actor = factory.Build<IDummyInterface2>(target);
            await actor.DoAsync();

            target.CallingThread.Should().Be(fiberThread);

            await fiber.DisposeAsync();
        }
    }
}
