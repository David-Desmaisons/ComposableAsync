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

        private const int ShortTimeOut = 100;
        private const int LongTimeOut = 2000;

        public CircuitBreakerPolicyTests()
        {
            _FakeAction = Substitute.For<Action>();
            _FakeTask = Substitute.For<Func<Task>>();
            _FakeFunction = Substitute.For<Func<int>>();
            _FakeTaskT = Substitute.For<Func<Task<int>>>();
            _ForAll = GetForAllShortTimeOut(5);
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
        public async Task ForAllException_Enqueue_Action_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Func_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeFunction.SetUpExceptions(10, 7, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            Func<Task<int>> @do = async () => await circuitBreaker.Enqueue(_FakeFunction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeFunction.Received(maxAttempts).Invoke();
            _FakeFunction.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeFunction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeTask.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeTask);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);
            await _FakeTask.Received(maxAttempts).Invoke();
            _FakeTask.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            await _FakeTask.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_T_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeTaskT.SetUpExceptions(10, 7, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);      
            Func<Task<int>> @do = async () => await circuitBreaker.Enqueue(_FakeTaskT);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);
            await _FakeTaskT.Received(maxAttempts).Invoke();
            _FakeTaskT.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            await _FakeTaskT.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Action_Stay_Open_During_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            async Task Do() => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeAction.ClearReceivedCalls();

            await AssertStayOpen(Do, 5);
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Func_Stay_Open_During_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeFunction.SetUpExceptions(10, 7, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            async Task Do() => await circuitBreaker.Enqueue(_FakeFunction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeFunction.ClearReceivedCalls();

            await AssertStayOpen(Do, 5);
            _FakeFunction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_Stay_Open_During_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeTask.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            async Task Do() => await circuitBreaker.Enqueue(_FakeTask);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeTask.ClearReceivedCalls();

            await AssertStayOpen(Do, 5);
            await _FakeTask.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_T_Stay_Open_During_Time_Out(Type exceptionType, int maxAttempts)
        {

            _FakeTaskT.SetUpExceptions(10, 55, exceptionType);
            var circuitBreaker = GetForAllLongTimeOut(maxAttempts);
            async Task Do() => await circuitBreaker.Enqueue(_FakeTaskT);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeTaskT.ClearReceivedCalls();

            await AssertStayOpen(Do, 5);
            await _FakeTaskT.DidNotReceive().Invoke();
        }

        private static async Task RetryAndExpectException<T>(Func<Task<T>> action, int replay, Type expected)
        {
            for (var count = 0; count < replay; count++)
            {
                (await action.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == expected);
            }
        }

        private static async Task RetryAndExpectException(Func<Task> action, int replay, Type expected)
        {
            for (var count = 0; count < replay; count++)
            {
                (await action.Should().ThrowAsync<Exception>()).Where(ex => ex.GetType() == expected);
            }
        }

        private static async Task AssertStayOpen<T>(Func<Task<T>> action, int replay)
        {
            for (var count = 0; count < replay; count++)
            {
                await action.Should().ThrowAsync<CircuitBreakerOpenException>();
            }
        }

        private static async Task AssertStayOpen(Func<Task> action, int replay)
        {
            for (var count = 0; count < replay; count++)
            {
                await action.Should().ThrowAsync<CircuitBreakerOpenException>();
            }
        }





        private IDispatcher GetForAllShortTimeOut(int maxAttempts)
        {
            return GetForAll(maxAttempts, ShortTimeOut);
        }

        private IDispatcher GetForAllLongTimeOut(int maxAttempts)
        {
            return GetForAll(maxAttempts, LongTimeOut);
        }

        private IDispatcher GetForAll(int maxAttempts, int timeOut)
        {
            return CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(maxAttempts, timeOut);
        }
    }
}
