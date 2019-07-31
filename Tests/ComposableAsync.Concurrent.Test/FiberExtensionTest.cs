using ComposableAsync.Concurrent.TaskSchedulers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Concurrent.Test
{
    public class FiberExtensionTest
    {
        [Fact]
        public void GetTaskScheduler_Returns_A_FiberTaskScheduler()
        {
            var fiber = Substitute.For<IFiber>();
            var res = fiber.GetTaskScheduler();
            res.Should().BeOfType<FiberTaskScheduler>();
        }
    }
}
