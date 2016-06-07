using System;
using System.Threading.Tasks;

using FluentAssertions;
 
using NSubstitute;

using EasyActor.TaskHelper;
using System.Threading;
using Xunit;

namespace EasyActor.Test.TaskHelper
{
     
    public class SynchronizationContextTaskSchedulerTest
    {
        [Fact]
        public void Constructor_Should_Throw_Exception_On_Null_Context()
        {
            SynchronizationContextTaskScheduler res = null;
            Action Do = () => res = new SynchronizationContextTaskScheduler(null);

            Do.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void MaximumConcurrencyLevel_Should_Be_1()
        {
            //Arrange
            var synContext = Substitute.For<SynchronizationContext>();
            var target = new SynchronizationContextTaskScheduler(synContext);

            //Assert
            target.MaximumConcurrencyLevel.Should().Be(1);
        }


        [Fact]
        public void GetScheduledTasksEnumerable_Should_Be_Null()
        {
            //Arrange
            var synContext = Substitute.For<SynchronizationContext>();
            var target = new SynchronizationContextTaskScheduler(synContext);

            //Assert
            target.GetScheduledTasksEnumerable().Should().BeNull();
        }


        private SynchronizationContext BuildSynchronizationContext()
        {
            var synContext = Substitute.For<SynchronizationContext>();
            synContext.When(sc => sc.Post(Arg.Any<SendOrPostCallback>(), Arg.Any<object>()))
                .Do(ci => Task.Run(() => ci.ArgAt<SendOrPostCallback>(0)(ci[1])));

            return synContext;
        }

        [Fact]
        public async Task Task_Created_By_Corresponding_Factory_Should_Call_SynchronizationContext_Post()
        {
            //Arrange
            var synContext = BuildSynchronizationContext();
            var target = new SynchronizationContextTaskScheduler(synContext);

            bool done = false;
            var factory = new TaskFactory(target);
            await factory.StartNew(() => done = true);

            //Assert
            synContext.Received().Post(Arg.Any<SendOrPostCallback>(), Arg.Any<object>());
            done.Should().BeTrue();
        }


        [Fact]
        public void Task_Created_By_Corresponding_Factory_Should_Call_SynchronizationContext_Post_On_Task_Inlining()
        {
            //Arrange
            var synContext = BuildSynchronizationContext();
            var target = new SynchronizationContextTaskScheduler(synContext);

            var done = false;
            var factory = new TaskFactory(target);

            factory.StartNew(
            () =>
            {
                factory.StartNew(() => done = true).Wait();
            }).Wait();

            //Assert
            synContext.Received().Post(Arg.Any<SendOrPostCallback>(), Arg.Any<object>());
            done.Should().BeTrue();
        }
    }
}
