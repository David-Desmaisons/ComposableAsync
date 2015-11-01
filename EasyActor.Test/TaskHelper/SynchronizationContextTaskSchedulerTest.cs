using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using NSubstitute;

using EasyActor.TaskHelper;
using System.Threading;


namespace EasyActor.Test.TaskHelper
{
    [TestFixture]
    public class SynchronizationContextTaskSchedulerTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Should_Throw_Exception_On_Null_Context()
        {
            var target = new SynchronizationContextTaskScheduler(null);
        }

        [Test]
        public void MaximumConcurrencyLevel_Should_Be_1()
        {
            //Arrange
            var synContext = Substitute.For<SynchronizationContext>();
            var target = new SynchronizationContextTaskScheduler(synContext);

            //Assert
            target.MaximumConcurrencyLevel.Should().Be(1);
        }


         [Test]
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
                .Do(ci => Task.Run(()=> ci.ArgAt<SendOrPostCallback>(0)(ci[1])));

            return synContext;
        }

        [Test]
        public async Task Task_Created_By_Corresponding_Factory_Should_Call_SynchronizationContext_Post()
        {
            //Arrange
            var synContext = BuildSynchronizationContext();
            var target = new SynchronizationContextTaskScheduler(synContext);

            bool done = false;
            var factory = new TaskFactory(target);
            await factory.StartNew(() => done = true);

            //Assert
            synContext.Received().Post(Arg.Any<SendOrPostCallback>(),Arg.Any<object>());
            done.Should().BeTrue();
        }

  
        [Test]
        public void Task_Created_By_Corresponding_Factory_Should_Call_SynchronizationContext_Post_On_Task_Inlining()
        {
            //Arrange
            var synContext = BuildSynchronizationContext();
            var target = new SynchronizationContextTaskScheduler(synContext);



            bool done = false;
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
