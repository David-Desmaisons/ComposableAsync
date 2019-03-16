using Concurrent.Tasks;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Tasks {
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
    }
}
