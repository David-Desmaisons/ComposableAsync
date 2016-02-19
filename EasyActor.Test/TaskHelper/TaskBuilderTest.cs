using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;

using EasyActor.TaskHelper;

namespace EasyActor.Test.TaskHelper
{
    [TestFixture]
    public class TaskBuilderTest
    {

        [Test]
        public void Cancelled_Should_Be_Cancelled()
        {
            var target = TaskBuilder.Cancelled;

            target.IsCanceled.Should().BeTrue();
        }

        [Test]
        public void Cancelled_T_Should_Be_Cancelled()
        {
            var target = TaskBuilder<int>.Cancelled;

            target.IsCanceled.Should().BeTrue();
        }


        [Test]
        public void Cancelled_Type_Task_T_Should_Be_Cancelled()
        {
            var target = TaskBuilder.GetCancelled(typeof(Task<int>));

            target.IsCanceled.Should().BeTrue();
            target.Should().BeOfType<Task<int>>();
        }

        [Test]
        public void Cancelled_Type_Task_Should_Be_Cancelled()
        {
            var target = TaskBuilder.GetCancelled(typeof(Task));

            target.IsCanceled.Should().BeTrue();
            target.Should().BeAssignableTo<Task>();
        }


        [Test]
        public void Cancelled_Type_Object_Should_Be_Null()
        {
            var target = TaskBuilder.GetCancelled(typeof(object));

            target.Should().BeNull();
        }

        [Test]
        public void Completed_Should_Be_Completed()
        {
            var target = TaskBuilder.Completed;

            target.IsCompleted.Should().BeTrue();
        }
    }
}
