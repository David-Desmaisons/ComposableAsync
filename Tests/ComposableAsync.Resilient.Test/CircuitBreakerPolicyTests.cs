using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using ComposableAsync.Resilient.Test.Helper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Resilient.Test
{
    public class CircuitBreakerPolicyTests
    {
        private readonly IDispatcher _ForAll;

        private readonly Action _FakeAction;
        private readonly Func<int> _FakeFunction;
        private readonly Func<Task> _FakeTask;
        private readonly Func<Task<int>> _FakeTaskT;

        private const int _TimeOut = 100;

        public CircuitBreakerPolicyTests()
        {
            _FakeAction = Substitute.For<Action>();
            _FakeTask = Substitute.For<Func<Task>>();
            _FakeFunction = Substitute.For<Func<int>>();
            _FakeTaskT = Substitute.For<Func<Task<int>>>();
            _ForAll = CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(5, _TimeOut);
        }

        [Fact]
        public async Task ForAllException_Enqueue_Action_Call_Action()
        {
            await _ForAll.Enqueue(_FakeAction);

            _FakeAction.Received(1).Invoke();
        }

        [Theory,AutoData]
        public async Task ForAllException_Enqueue_Func_Call_Func(int result)
        {
            _FakeFunction.Invoke().Returns(result);
            var actual = await _ForAll.Enqueue(_FakeFunction);

            actual.Should().Be(result);
            _FakeFunction.Received(1).Invoke();
        }

        [Fact]
        public async Task ForAllException_Enqueue_Task_Call_Func_Task()
        {
            await _ForAll.Enqueue(_FakeTask);

            await _FakeTask.Received(1).Invoke();
        }

        [Theory,AutoData]
        public async Task ForAllException_Enqueue_Task_Call_Func_Task_T(int result)
        {
            _FakeTaskT.Invoke().Returns(Task.FromResult(result));
            var actual = await _ForAll.Enqueue(_FakeTaskT);

            actual.Should().Be(result);
            await _FakeTaskT.Received(1).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(ArgumentNullException))]
        [InlineData(typeof(SystemException))]
        [InlineData(typeof(Exception))]
        public async Task ForAllException_Enqueue_Action_Rethrow_Exception(Type exceptionType)
        {
            _FakeAction.SetUpExceptions(1, exceptionType);
            Func<Task> @do = async () => await _ForAll.Enqueue(_FakeAction);

            (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
        }

        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(ArgumentNullException))]
        [InlineData(typeof(SystemException))]
        [InlineData(typeof(Exception))]
        public async Task ForAllException_Enqueue_Func_Rethrow_Exception(Type exceptionType)
        {
            _FakeFunction.SetUpExceptions(1, 7, exceptionType);
            Func<Task<int>> @do = async () => await _ForAll.Enqueue(_FakeFunction);

            (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
        }

        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(ArgumentNullException))]
        [InlineData(typeof(SystemException))]
        [InlineData(typeof(Exception))]
        public async Task ForAllException_Enqueue_Task_Rethrow_Exception(Type exceptionType)
        {
            _FakeTask.SetUpExceptions(1, exceptionType);
            Func<Task> @do = async () => await _ForAll.Enqueue(_FakeTask);

            (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
        }

        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(ArgumentNullException))]
        [InlineData(typeof(SystemException))]
        [InlineData(typeof(Exception))]
        public async Task ForAllException_Enqueue_Task_T_Rethrow_Exception(Type exceptionType)
        {
            _FakeTaskT.SetUpExceptions(1, 4, exceptionType);
            Func<Task<int>> @do = async () => await _ForAll.Enqueue(_FakeTaskT);

            (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Action_Reach_Open_State_When_MaxAttemps_Reached(Type exceptionType, int maxAttemps)
        {
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(maxAttemps, _TimeOut);

            _FakeAction.SetUpExceptions(10, exceptionType);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);
            for(var count=0; count< maxAttemps; count++)
            {
                (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
            }
            _FakeAction.Received(maxAttemps).Invoke();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeAction.Received(maxAttemps).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Func_Reach_Open_State_When_MaxAttemps_Reached(Type exceptionType, int maxAttemps)
        {
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(maxAttemps, _TimeOut);

            _FakeFunction.SetUpExceptions(10, 7, exceptionType);
            Func<Task<int>> @do = async () => await circuitBreaker.Enqueue(_FakeFunction);
            for(var count=0; count< maxAttemps; count++)
            {
                (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
            }
            _FakeFunction.Received(maxAttemps).Invoke();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeFunction.Received(maxAttemps).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_Reach_Open_State_When_MaxAttemps_Reached(Type exceptionType, int maxAttemps)
        {
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(maxAttemps, _TimeOut);

            _FakeTask.SetUpExceptions(10, exceptionType);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeTask);
            for(var count=0; count< maxAttemps; count++)
            {
                (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
            }
            await _FakeTask.Received(maxAttemps).Invoke();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            await _FakeTask.Received(maxAttemps).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_T_Reach_Open_State_When_MaxAttemps_Reached(Type exceptionType, int maxAttemps)
        {
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(maxAttemps, _TimeOut);

            _FakeTaskT.SetUpExceptions(10, 7, exceptionType);
            Func<Task<int>> @do = async () => await circuitBreaker.Enqueue(_FakeTaskT);
            for(var count=0; count< maxAttemps; count++)
            {
                (await @do.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
            }
            await _FakeTaskT.Received(maxAttemps).Invoke();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            await _FakeTaskT.Received(maxAttemps).Invoke();
        }
    }
}
