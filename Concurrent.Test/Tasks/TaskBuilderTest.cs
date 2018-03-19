using System.Threading.Tasks;
using Concurrent.Tasks;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Tasks
{  
    public class TaskBuilderTest
    {

        [Fact]
        public void Cancelled_Should_Be_Cancelled()
        {
            var target = TaskBuilder.Cancelled;

            target.IsCanceled.Should().BeTrue();
        }

        [Fact]
        public void Cancelled_T_Should_Be_Cancelled()
        {
            var target = TaskBuilder<int>.Cancelled;

            target.IsCanceled.Should().BeTrue();
        }


        [Fact]
        public void Cancelled_Type_Task_T_Should_Be_Cancelled()
        {
            var target = typeof(Task<int>).GetCancelled();

            target.IsCanceled.Should().BeTrue();
            target.Should().BeOfType<Task<int>>();
        }

        [Fact]
        public void Cancelled_Type_Task_Should_Be_Cancelled()
        {
            var target = typeof(Task).GetCancelled();

            target.IsCanceled.Should().BeTrue();
            target.Should().BeAssignableTo<Task>();
        }


        [Fact]
        public void Cancelled_Type_Object_Should_Be_Null()
        {
            var target = typeof(object).GetCancelled();

            target.Should().BeNull();
        }

        [Fact]
        public void Completed_Should_Be_Completed()
        {
            var target = TaskBuilder.Completed;

            target.IsCompleted.Should().BeTrue();
        }
    }
}
