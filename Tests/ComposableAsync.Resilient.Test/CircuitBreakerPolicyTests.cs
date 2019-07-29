using AutoFixture.Xunit2;
using ComposableAsync.Resilient.Test.Helper;
using FluentAssertions;
using NSubstitute;
using System;
using System.Threading.Tasks;
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

        [Theory, AutoData]
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

        [Theory, AutoData]
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

            await @do.Should().ThrowAsync(exceptionType);
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

            await @do.Should().ThrowAsync(exceptionType);
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

            await @do.Should().ThrowAsync(exceptionType);
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

            await @do.Should().ThrowAsync(exceptionType);
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

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Action_Retry_After_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetForAllShortTimeOut(maxAttempts);
            Func<Task> Do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeAction.ClearReceivedCalls();
            await Task.Delay(ShortTimeOut);

            await Do.Should().ThrowAsync(exceptionType);
            _FakeAction.Received(1).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Func_Retry_After_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeFunction.SetUpExceptions(10, 7, exceptionType);
            var circuitBreaker = GetForAllShortTimeOut(maxAttempts);
            Func<Task> Do = async () => await circuitBreaker.Enqueue(_FakeFunction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeFunction.ClearReceivedCalls();
            await Task.Delay(ShortTimeOut);

            await Do.Should().ThrowAsync(exceptionType);
            _FakeFunction.Received(1).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_Retry_After_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeTask.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetForAllShortTimeOut(maxAttempts);
            Func<Task> Do = async () => await circuitBreaker.Enqueue(_FakeTask);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeTask.ClearReceivedCalls();
            await Task.Delay(ShortTimeOut);

            await Do.Should().ThrowAsync(exceptionType);
            await _FakeTask.Received(1).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_Enqueue_Task_T_Retry_After_Time_Out(Type exceptionType, int maxAttempts)
        {
            _FakeTaskT.SetUpExceptions(10, 55, exceptionType);
            var circuitBreaker = GetForAllShortTimeOut(maxAttempts);
            Func<Task> Do = async () => await circuitBreaker.Enqueue(_FakeTaskT);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);
            _FakeTaskT.ClearReceivedCalls();
            await Task.Delay(ShortTimeOut);

            await Do.Should().ThrowAsync(exceptionType);
            await _FakeTaskT.Received(1).Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        public async Task ForException_Enqueue_Action_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetShortTimeOut<ArgumentException>(maxAttempts);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForException_Enqueue_Action_Do_Not_Reach_Open_State_When_Exception_Is_Not_Matching(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = GetShortTimeOut<ArgumentException>(maxAttempts);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync(exceptionType);
            _FakeAction.Received(1).Invoke();
        }

        [Theory]
        [InlineAutoData(typeof(ArgumentException), 1)]
        [InlineAutoData(typeof(Exception), 3)]
        public async Task ForExceptionFilter_Enqueue_Action_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts, string message)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType, message);
            _FakeAction.SetUpExceptions(10, exception);
            var circuitBreaker = GetCircuitBreaker(maxAttempts, message, LongTimeOut);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineAutoData(typeof(BadImageFormatException), 3)]
        [InlineAutoData(typeof(SystemException), 2)]
        [InlineAutoData(typeof(Exception), 1)]
        public async Task ForExceptionFilter_Enqueue_Action_Do_Not_Reach_Open_State_When_Exception_Is_Not_Matching(Type exceptionType, int maxAttempts, string message)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType, $"{message}-different");
            _FakeAction.SetUpExceptions(10, exception);
            var circuitBreaker = GetCircuitBreaker(maxAttempts, message, LongTimeOut);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync(exceptionType);
            _FakeAction.Received(1).Invoke();
        }

        [Theory]
        [InlineAutoData(1)]
        [InlineAutoData(3)]
        public async Task ForExceptionFilterGeneric_Enqueue_Action_Reach_Open_State_When_MaxAttempts_Reached(int maxAttempts, string message)
        {
            var exception = new ArgumentException(message);
            _FakeAction.SetUpExceptions(10, exception);
            var circuitBreaker = GetCircuitBreaker<ArgumentException>(maxAttempts, message, LongTimeOut);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exception.GetType());

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(2)]
        [InlineAutoData(1)]
        public async Task ForExceptionFilterGeneric_Enqueue_Action_Do_Not_Reach_Open_State_When_Exception_Is_Not_Matching_Filter(int maxAttempts, string message)
        {
            var exception = new ArgumentException($"{message}-suffix");
            _FakeAction.SetUpExceptions(10, exception);
            var circuitBreaker = GetCircuitBreaker<ArgumentException>(maxAttempts, message, LongTimeOut);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exception.GetType());

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync(exception.GetType());
            _FakeAction.Received(1).Invoke();
        }

        [Theory]
        [InlineAutoData(typeof(BadImageFormatException), 3)]
        [InlineAutoData(typeof(SystemException), 2)]
        [InlineAutoData(typeof(Exception), 1)]
        public async Task ForExceptionFilterGeneric_Enqueue_Action_Do_Not_Reach_Open_State_When_Exception_Is_Not_Matching(Type exceptionType, int maxAttempts, string message)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType, message);
            _FakeAction.SetUpExceptions(10, exception);
            var circuitBreaker = GetCircuitBreaker<ArgumentException>(maxAttempts, message, LongTimeOut);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync(exceptionType);
            _FakeAction.Received(1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        public async Task ForException_And_Enqueue_Action_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.For<BadImageFormatException>().And<ArgumentNullException>().WithRetryAndTimeout(maxAttempts, ShortTimeOut);
            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync<CircuitBreakerOpenException>();
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForException_And_Enqueue_Action_Do_Not_Reach_Open_State_When_Exception_Is_Not_Matching(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.For<BadImageFormatException>().And<ArgumentNullException>().WithRetryAndTimeout(maxAttempts, TimeSpan.FromMilliseconds(ShortTimeOut));

            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().ThrowAsync(exceptionType);
            _FakeAction.Received(1).Invoke();
        }


        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_DoNotThrowForVoid_Enqueue_Action_Do_Not_Throw_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeAction.SetUpExceptions(10, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().DoNotThrowForVoid().WithRetryAndTimeout(maxAttempts, LongTimeOut);

            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeAction);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            _FakeAction.Received(maxAttempts).Invoke();
            _FakeAction.ClearReceivedCalls();

            await @do.Should().NotThrowAsync();
            _FakeAction.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_DoNotThrowForVoid_Enqueue_Task_Do_Not_Throw_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeTask.SetUpExceptions(10, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().DoNotThrowForVoid().WithRetryAndTimeout(maxAttempts, TimeSpan.FromMilliseconds(ShortTimeOut));

            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeTask);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            await _FakeTask.Received(maxAttempts).Invoke();
            _FakeTask.ClearReceivedCalls();

            await @do.Should().NotThrowAsync();
            await _FakeTask.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        public async Task ForException_DoNotThrowForVoid_Enqueue_Task_Do_Not_Throw_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeTask.SetUpExceptions(10, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.For<SystemException>().DoNotThrowForVoid().WithRetryAndTimeout(maxAttempts, TimeSpan.FromMilliseconds(LongTimeOut));

            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeTask);

            await RetryAndExpectException(@do, maxAttempts, exceptionType);

            await _FakeTask.Received(maxAttempts).Invoke();
            _FakeTask.ClearReceivedCalls();

            await @do.Should().NotThrowAsync();
            await _FakeTask.DidNotReceive().Invoke();
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_DoNotThrowForVoid_Enqueue_Func_Do_Throw_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeFunction.SetUpExceptions(10, 5, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().DoNotThrowForVoid().WithRetryAndTimeout(maxAttempts, LongTimeOut);

            Func<Task> @do = async () => await circuitBreaker.Enqueue(_FakeFunction);

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
        public async Task ForAllException_ReturnsDefaultWhenOpen_Enqueue_Func_Return_Default_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeFunction.SetUpExceptions(10, 50, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().ReturnsDefaultWhenOpen().WithRetryAndTimeout(maxAttempts, LongTimeOut);

            async Task Do() => await circuitBreaker.Enqueue(_FakeFunction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);

            _FakeFunction.Received(maxAttempts).Invoke();
            _FakeFunction.ClearReceivedCalls();

            var res = await circuitBreaker.Enqueue(_FakeFunction);
            _FakeFunction.DidNotReceive().Invoke();
            res.Should().Be(0);
        }

        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        public async Task ForException_ReturnsDefaultWhenOpen_Enqueue_Func_Return_Default_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeFunction.SetUpExceptions(10, 50, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.For<SystemException>().ReturnsDefaultWhenOpen().WithRetryAndTimeout(maxAttempts, LongTimeOut);

            async Task Do() => await circuitBreaker.Enqueue(_FakeFunction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);

            _FakeFunction.Received(maxAttempts).Invoke();
            _FakeFunction.ClearReceivedCalls();

            var res = await circuitBreaker.Enqueue(_FakeFunction);
            _FakeFunction.DidNotReceive().Invoke();
            res.Should().Be(0);
        }

        [Theory]
        [InlineAutoData(typeof(ArgumentException), 1)]
        [InlineAutoData(typeof(ArgumentNullException), 3)]
        [InlineAutoData(typeof(SystemException), 2)]
        public async Task ForException_ReturnsWhenOpen_Enqueue_Func_Return_Value_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts, int expected)
        {
            _FakeFunction.SetUpExceptions(10, 50, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.For<SystemException>().ReturnsWhenOpen(expected).WithRetryAndTimeout(maxAttempts, LongTimeOut);

            async Task Do() => await circuitBreaker.Enqueue(_FakeFunction);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);

            _FakeFunction.Received(maxAttempts).Invoke();
            _FakeFunction.ClearReceivedCalls();

            var res = await circuitBreaker.Enqueue(_FakeFunction);
            _FakeFunction.DidNotReceive().Invoke();
            res.Should().Be(expected);
        }


        [Theory]
        [InlineData(typeof(ArgumentException), 1)]
        [InlineData(typeof(ArgumentNullException), 3)]
        [InlineData(typeof(SystemException), 2)]
        [InlineData(typeof(Exception), 1)]
        public async Task ForAllException_ReturnsDefaultWhenOpen_Enqueue_Task_T_Return_Default_When_Reach_Open_State_When_MaxAttempts_Reached(Type exceptionType, int maxAttempts)
        {
            _FakeTaskT.SetUpExceptions(10, 50, exceptionType);
            var circuitBreaker = CircuitBreakerPolicy.ForAllException().ReturnsDefaultWhenOpen().WithRetryAndTimeout(maxAttempts, LongTimeOut);

            async Task Do() => await circuitBreaker.Enqueue(_FakeTaskT);

            await RetryAndExpectException(Do, maxAttempts, exceptionType);

            await _FakeTaskT.Received(maxAttempts).Invoke();
            _FakeTaskT.ClearReceivedCalls();

            var res = await circuitBreaker.Enqueue(_FakeTaskT);
            await _FakeTaskT.DidNotReceive().Invoke();
            res.Should().Be(0);
        }

        private static async Task RetryAndExpectException<T>(Func<Task<T>> action, int replay, Type expected)
        {
            for (var count = 0; count < replay; count++)
            {
                await action.Should().ThrowAsync(expected);
            }
        }

        private static async Task RetryAndExpectException(Func<Task> action, int replay, Type expected)
        {
            for (var count = 0; count < replay; count++)
            {
                await action.Should().ThrowAsync(expected);
            }
        }

        private static async Task AssertStayOpen(Func<Task> action, int replay)
        {
            for (var count = 0; count < replay; count++)
            {
                await action.Should().ThrowAsync<CircuitBreakerOpenException>();
            }
        }

        private static IDispatcher GetForAllShortTimeOut(int maxAttempts)
        {
            return GetForAll(maxAttempts, ShortTimeOut);
        }

        private static IDispatcher GetForAllLongTimeOut(int maxAttempts)
        {
            return GetForAll(maxAttempts, LongTimeOut);
        }

        private static IDispatcher GetForAll(int maxAttempts, int timeOut)
        {
            return CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(maxAttempts, timeOut);
        }

        private static IDispatcher GetShortTimeOut<T>(int maxAttempts) where T : Exception
        {
            return GetCircuitBreaker<T>(maxAttempts, ShortTimeOut);
        }

        private static IDispatcher GetCircuitBreaker<T>(int maxAttempts, int timeOut) where T : Exception
        {
            return CircuitBreakerPolicy.For<T>().WithRetryAndTimeout(maxAttempts, timeOut);
        }

        private static IDispatcher GetCircuitBreaker(int maxAttempts, string message, int timeOut)
        {
            return CircuitBreakerPolicy.ForException(ex => ex.Message == message).WithRetryAndTimeout(maxAttempts, timeOut);
        }

        private static IDispatcher GetCircuitBreaker<T>(int maxAttempts, string message, int timeOut) where T : Exception
        {
            return CircuitBreakerPolicy.For<T>(ex => ex.Message == message).WithRetryAndTimeout(maxAttempts, timeOut);
        }
    }
}
