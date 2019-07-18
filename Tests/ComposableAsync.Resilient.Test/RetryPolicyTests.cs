using AutoFixture.Xunit2;
using ComposableAsync.Resilient.Test.Helper;
using FluentAssertions;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ComposableAsync.Resilient.Test
{
    public class RetryPolicyTests
    {
        private readonly IDispatcher _ForAllForEver;
        private readonly IDispatcher _ForAllForEverWithTimeOut;
        private readonly IDispatcher _ForNullReferenceExceptionForEver;
        private readonly IDispatcher _ForArgumentExceptionWithTimeOut;

        private readonly Action _FakeAction;
        private readonly Func<int> _FakeFunction;
        private readonly Func<Task> _FakeTask;
        private readonly Func<Task<int>> _FakeTaskT;

        private readonly int _TimeOut =200;

        public RetryPolicyTests()
        {
            _FakeAction = Substitute.For<Action>();
            _FakeTask = Substitute.For<Func<Task>>();
            _FakeFunction = Substitute.For<Func<int>>();
            _FakeTaskT = Substitute.For<Func<Task<int>>>();
            _ForAllForEver = RetryPolicy.ForAllException().ForEver();
            _ForAllForEverWithTimeOut = RetryPolicy.ForAllException().WithWaitBetweenRetry(_TimeOut).ForEver();
            _ForNullReferenceExceptionForEver = RetryPolicy.For<NullReferenceException>().ForEver();
            _ForArgumentExceptionWithTimeOut = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(_TimeOut).ForEver();
        }

        #region ForAll

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForAllException_Enqueue_Task_Calls_TillNoException(int times)
        {
            _FakeTask.SetUpExceptions(times);
            await _ForAllForEver.Enqueue(_FakeTask);
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void ForAllException_Dispatch_Calls_TillNoException(int times)
        {
            _FakeAction.SetUpExceptions(times);
            _ForAllForEver.Dispatch(_FakeAction);
            _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForAllException_Enqueue_Action_Calls_TillNoException(int times)
        {
            _FakeAction.SetUpExceptions(times);
            await _ForAllForEver.Enqueue(_FakeAction);
            _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllException_Enqueue_Action_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeAction.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource);
            Func<Task> @do = async () => await _ForAllForEver.Enqueue(_FakeAction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeAction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForAllException_Enqueue_Func_Calls_TillNoException(int times, int res)
        {
            _FakeFunction.SetUpExceptions(times, res);
            var actual = await _ForAllForEver.Enqueue(_FakeFunction);

            actual.Should().Be(res);
            _FakeFunction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllException_Enqueue_Func_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeFunction.SetUpExceptionsWithCancellation(times, timesToCancel, 8, tokenSource);
            Func<Task> @do = async () => await _ForAllForEver.Enqueue(_FakeFunction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeFunction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(NullReferenceException))]
        [InlineData(typeof(ArgumentException))]
        public async Task ForAllException_Enqueue_Task_CatchAllException(Type exceptionType)
        {
            _FakeTask.SetUpExceptions(5, exceptionType);
            await _ForAllForEver.Enqueue(_FakeTask);
            await _FakeTask.Received(6).Invoke();
        }

        [Theory]
        [InlineAutoData(typeof(BadImageFormatException))]
        [InlineAutoData(typeof(NullReferenceException))]
        [InlineAutoData(typeof(ArgumentException))]
        public async Task ForAllException_Enqueue_Task_T_CatchAllException(Type exceptionType, int res)
        {
            _FakeTaskT.SetUpExceptions(5, res, exceptionType);
            await _ForAllForEver.Enqueue(_FakeTaskT);
            await _FakeTaskT.Received(6).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllException_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTask.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource);
            Func<Task> @do = async () => await _ForAllForEver.Enqueue(_FakeTask, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTask.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForAllException_Enqueue_Task_T__Calls_TillNoException(int times, int value)
        {
            _FakeTaskT.SetUpExceptions(times, value);
            var res = await _ForAllForEver.Enqueue(_FakeTaskT);

            res.Should().Be(value);
            await _FakeTaskT.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(4)]
        [InlineAutoData(10)]
        public async Task ForAllException_Enqueue_Task_T_Can_BeCancelled(int times, int res)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTaskT.SetUpExceptionsWithCancellation(times, timesToCancel, res, tokenSource);
            Func<Task> @do = async () => await _ForAllForEver.Enqueue(_FakeTaskT, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTaskT.Received(timesToCancel + 1).Invoke();
        }

        #endregion

        #region ForAll WithWaitBetweenRetry

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_Calls_TillNoException(int times)
        {
            _FakeTask.SetUpExceptions(times);
            await _ForAllForEverWithTimeOut.Enqueue(_FakeTask);
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_Respect_TimeOut(int times)
        {
            _FakeTask.SetUpExceptions(times);
            var watch = Stopwatch.StartNew();
            await _ForAllForEverWithTimeOut.Enqueue(_FakeTask);
            watch.Stop();
            watch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(times * _TimeOut), 100);
        }

        [Theory]
        [InlineData(1, 100)]
        [InlineData(2, 300)]
        [InlineData(3, 600)]
        [InlineData(4, 700)]
        public async Task ForAllException_WithWait_Enqueue_Action_TillNoException_WithTimeoutSeries(int times, int expectedTimeInMs)
        {
            var replay = RetryPolicy.ForAllException().WithWaitBetweenRetry(100, 200, 300, 100).ForEver();
            _FakeAction.SetUpExceptions(times);
            var watch = Stopwatch.StartNew();
            await replay.Enqueue(_FakeAction);
            watch.Stop();
            watch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(expectedTimeInMs), 50);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForAllForEverWithTimeOut_Action_Calls_TillNoException(int times)
        {
            _FakeAction.SetUpExceptions(times);
            await _ForAllForEverWithTimeOut.Enqueue(_FakeAction);
            _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Action_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeAction.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource);
            Func<Task> @do = async () => await _ForAllForEverWithTimeOut.Enqueue(_FakeAction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeAction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Func_Calls_TillNoException(int times, int res)
        {
            _FakeFunction.SetUpExceptions(times, res);
            var actual = await _ForAllForEverWithTimeOut.Enqueue(_FakeFunction);

            actual.Should().Be(res);
            _FakeFunction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Func_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeFunction.SetUpExceptionsWithCancellation(times, timesToCancel, 8, tokenSource);
            Func<Task> @do = async () => await _ForAllForEverWithTimeOut.Enqueue(_FakeFunction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeFunction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(NullReferenceException))]
        [InlineData(typeof(ArgumentException))]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_CatchAllException(Type exceptionType)
        {
            _FakeTask.SetUpExceptions(5, exceptionType);
            await _ForAllForEverWithTimeOut.Enqueue(_FakeTask);
            await _FakeTask.Received(6).Invoke();
        }

        [Theory]
        [InlineAutoData(typeof(BadImageFormatException))]
        [InlineAutoData(typeof(NullReferenceException))]
        [InlineAutoData(typeof(ArgumentException))]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_T_CatchAllException(Type exceptionType, int res)
        {
            _FakeTaskT.SetUpExceptions(5, res, exceptionType);
            await _ForAllForEverWithTimeOut.Enqueue(_FakeTaskT);
            await _FakeTaskT.Received(6).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTask.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource);
            Func<Task> @do = async () => await _ForAllForEverWithTimeOut.Enqueue(_FakeTask, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTask.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_T__Calls_TillNoException(int times, int value)
        {
            _FakeTaskT.SetUpExceptions(times, value);
            var res = await _ForAllForEverWithTimeOut.Enqueue(_FakeTaskT);

            res.Should().Be(value);
            await _FakeTaskT.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(4)]
        [InlineAutoData(10)]
        public async Task ForAllForEverWithTimeOut_Enqueue_Task_T_Can_BeCancelled(int times, int res)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTaskT.SetUpExceptionsWithCancellation(times, timesToCancel, res, tokenSource);
            Func<Task> @do = async () => await _ForAllForEverWithTimeOut.Enqueue(_FakeTaskT, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTaskT.Received(timesToCancel + 1).Invoke();
        }

        #endregion

        #region ForAll WithMaxRetry

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(5, 10)]
        [InlineData(10, 20)]
        public async Task ForAllException_WithMax_Enqueue_Action_DoesNotThrow_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            _FakeAction.SetUpExceptions(times);
            Func<Task> @do = async () => await replay.Enqueue(_FakeAction);
            await @do.Should().NotThrowAsync();
            _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(10, 5)]
        [InlineData(1000, 3)]
        public async Task ForAllException_WithMax_Enqueue_Action_TillNoNullException_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            var expectedType = typeof(BadImageFormatException);
            _FakeAction.SetUpExceptions(times, expectedType);
            Func<Task> @do = async () => await replay.Enqueue(_FakeAction);
            await @do.Should().ThrowAsync<BadImageFormatException>();
            _FakeAction.Received(maxRetry + 1).Invoke();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(10, 5)]
        [InlineData(1000, 3)]
        public async Task ForAllException_WithWait_WithMax_Enqueue_Action_TillNoNullException_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.ForAllException().WithWaitBetweenRetry(_TimeOut).WithMaxRetry(maxRetry);
            var expectedType = typeof(BadImageFormatException);
            _FakeAction.SetUpExceptions(times, expectedType);
            Func<Task> @do = async () => await replay.Enqueue(_FakeAction);
            var watch = Stopwatch.StartNew();
            await @do.Should().ThrowAsync<BadImageFormatException>();
            watch.Stop();
            watch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(_TimeOut* maxRetry), 100);
        }

        [Fact]
        public async Task ForAllException_WithWait_WithMax_Enqueue_Action_Is_Cancellable()
        {
            var timeOut = TimeSpan.FromMilliseconds(100);
            var replay = RetryPolicy.ForAllException().WithWaitBetweenRetry(TimeSpan.FromMinutes(2)).ForEver();
            var expectedType = typeof(BadImageFormatException);
            _FakeAction.SetUpExceptions(1000, expectedType);
            var tokenSource = new CancellationTokenSource(timeOut);
            Func<Task> @do = async () => await replay.Enqueue(_FakeAction, tokenSource.Token);
            var watch = Stopwatch.StartNew();
            await @do.Should().ThrowAsync<TaskCanceledException>();
            watch.Stop();
            watch.Elapsed.Should().BeCloseTo(timeOut, 50);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(5, 10)]
        [InlineData(10, 20)]
        public async Task ForAllException_WithMax_Enqueue_Func_DoesNotThrow_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            _FakeFunction.SetUpExceptions(times, 20);
            Func<Task<int>> @do = async () => await replay.Enqueue(_FakeFunction);
            await @do.Should().NotThrowAsync();
            _FakeFunction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(10, 5)]
        [InlineData(1000, 3)]
        public async Task ForAllException_WithMax_Enqueue_Func_TillNoNullException_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            var expectedType = typeof(BadImageFormatException);
            _FakeFunction.SetUpExceptions(times, 5, expectedType);
            Func<Task> @do = async () => await replay.Enqueue(_FakeFunction);
            await @do.Should().ThrowAsync<BadImageFormatException>();
            _FakeFunction.Received(maxRetry + 1).Invoke();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(5, 10)]
        [InlineData(10, 20)]
        public async Task ForAllException_WithMax_Enqueue_Task_DoesNotThrow_WhenLessThanMaxRetry(int times, int maxRetry) {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            _FakeTask.SetUpExceptions(times, typeof(NullReferenceException));
            Func<Task> @do = async () => await replay.Enqueue(_FakeTask);
            await @do.Should().NotThrowAsync();
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(10, 5)]
        [InlineData(1000, 3)]
        public async Task ForAllException_WithMax_Enqueue_Task_TillNoNullException_WhenLessThanMaxRetry(int times, int maxRetry) {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            var expectedType = typeof(BadImageFormatException);
            _FakeTask.SetUpExceptions(times, expectedType);
            Func<Task> @do = async () => await replay.Enqueue(_FakeTask);
            await @do.Should().ThrowAsync<BadImageFormatException>();
            await _FakeTask.Received(maxRetry + 1).Invoke();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(5, 10)]
        [InlineData(10, 20)]
        public async Task ForAllException_WithMax_Enqueue_Task_T_DoesNotThrow_WhenLessThanMaxRetry(int times, int maxRetry) {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            _FakeTaskT.SetUpExceptions(times, 1);
            Func<Task<int>> @do = async () => await replay.Enqueue(_FakeTaskT);
            await @do.Should().NotThrowAsync();
            await _FakeTaskT.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]
        [InlineData(10, 5)]
        [InlineData(1000, 3)]
        public async Task ForAllException_WithMax_Enqueue_Task_T_TillNoNullException_WhenLessThanMaxRetry(int times, int maxRetry) {
            var replay = RetryPolicy.ForAllException().WithMaxRetry(maxRetry);
            var expectedType = typeof(BadImageFormatException);
            _FakeTaskT.SetUpExceptions(times, 6, expectedType);
            Func<Task<int>> @do = async () => await replay.Enqueue(_FakeTaskT);
            await @do.Should().ThrowAsync<BadImageFormatException>();
            await _FakeTaskT.Received(maxRetry + 1).Invoke();
        }

        #endregion

        #region Selective

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_TillNoNullException(int times)
        {
            _FakeTask.SetUpExceptions(times, typeof(NullReferenceException));
            await _ForNullReferenceExceptionForEver.Enqueue(_FakeTask);
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void ForException_Dispatch_Calls_TillNoException(int times)
        {
            _FakeAction.SetUpExceptions(times, typeof(NullReferenceException));
            _ForNullReferenceExceptionForEver.Dispatch(_FakeAction);
            _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForException_Enqueue_Func_Calls_TillNoException(int times, int res)
        {
            _FakeFunction.SetUpExceptions(times, res, typeof(NullReferenceException));
            var actual = await _ForNullReferenceExceptionForEver.Enqueue(_FakeFunction);

            actual.Should().Be(res);
            _FakeFunction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(IndexOutOfRangeException))]
        [InlineData(typeof(ArgumentException))]
        public async Task ForException_Enqueue_Action_IsSelective(Type type)
        {
            _FakeAction.SetUpExceptions(1, type);
            Func<Task> @do = async () =>
                await _ForNullReferenceExceptionForEver.Enqueue(_FakeAction, CancellationToken.None);
            var exceptionAssertions = await @do.Should().ThrowAsync<Exception>();
            exceptionAssertions.Where(ex => ex.GetType() == type);
        }

        [Theory]
        [InlineAutoData(typeof(BadImageFormatException))]
        [InlineAutoData(typeof(IndexOutOfRangeException))]
        [InlineAutoData(typeof(ArgumentException))]
        public async Task ForException_Enqueue_Func_IsSelective(Type type, int res)
        {
            _FakeFunction.SetUpExceptions(1, res, type);
            Func<Task> @do = async () =>
                await _ForNullReferenceExceptionForEver.Enqueue(_FakeFunction, CancellationToken.None);
            var exceptionAssertions = await @do.Should().ThrowAsync<Exception>();
            exceptionAssertions.Where(ex => ex.GetType() == type);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Func_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeFunction.SetUpExceptionsWithCancellation(times, timesToCancel, 8, tokenSource,
                typeof(NullReferenceException));
            Func<Task> @do = async () =>
                await _ForNullReferenceExceptionForEver.Enqueue(_FakeFunction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeFunction.Received(timesToCancel + 1).Invoke();
        }

        private class ChildNullReferenceException : NullReferenceException
        {
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_TillNoDerivedNullException(int times)
        {
            _FakeTask.SetUpExceptions(times, typeof(ChildNullReferenceException));
            await _ForNullReferenceExceptionForEver.Enqueue(_FakeTask);
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(NullReferenceException))]
        [InlineData(typeof(ChildNullReferenceException))]
        public async Task ForException_And_Enqueue_Task_TillNoExceptionsRegistered(Type type)
        {
            var times = 4;
            var composed = RetryPolicy.For<BadImageFormatException>().And<NullReferenceException>().ForEver();
            _FakeTask.SetUpExceptions(times, type);
            await composed.Enqueue(_FakeTask);
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTask.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource,
                typeof(NullReferenceException));
            Func<Task> @do = async () => await _ForNullReferenceExceptionForEver.Enqueue(_FakeTask, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTask.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForException_Enqueue_Task_T_TillNoException(int times, int value)
        {
            _FakeTaskT.SetUpExceptions(times, value, typeof(NullReferenceException));

            var res = await _ForNullReferenceExceptionForEver.Enqueue(_FakeTaskT);

            res.Should().Be(value);
            await _FakeTaskT.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(4)]
        [InlineAutoData(10)]
        public async Task ForException_Enqueue_Task_T_Can_BeCancelled(int times, int res)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTaskT.SetUpExceptionsWithCancellation(times, timesToCancel, res, tokenSource,
                typeof(NullReferenceException));

            Func<Task> @do = async () => await _ForNullReferenceExceptionForEver.Enqueue(_FakeTaskT, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTaskT.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(Exception))]
        public async Task ForException_Enqueue_Only_Selected_Exception(Type exceptionType)
        {
            _FakeTask.SetUpExceptions(1, exceptionType);
            Func<Task> @do = async () => await _ForNullReferenceExceptionForEver.Enqueue(_FakeTask);
            var expected = await @do.Should().ThrowAsync<Exception>();
            expected.Where(ex => ex.GetType() == exceptionType);
        }

        #endregion

        #region Selective WithWaitBetweenRetry

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Task_Calls_TillNoException(int times)
        {
            _FakeTask.SetUpExceptions(times, typeof(ArgumentException));
            await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeTask);
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Task_Respect_TimeOut(int times)
        {
            _FakeTask.SetUpExceptions(times, typeof(ArgumentException));
            var watch = Stopwatch.StartNew();
            await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeTask);
            watch.Stop();
            watch.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(times * _TimeOut), 100);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForExceptionForEverWithTimeOut_Action_Calls_TillNoException(int times)
        {
            _FakeAction.SetUpExceptions(times, typeof(ArgumentException));
            await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeAction);
            _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Action_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeAction.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource, typeof(ArgumentException));
            Func<Task> @do = async () => await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeAction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeAction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Func_Calls_TillNoException(int times, int res)
        {
            _FakeFunction.SetUpExceptions(times, res, typeof(ArgumentException));
            var actual = await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeFunction);

            actual.Should().Be(res);
            _FakeFunction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Func_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeFunction.SetUpExceptionsWithCancellation(times, timesToCancel, 8, tokenSource, typeof(ArgumentException));
            Func<Task> @do = async () => await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeFunction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            _FakeFunction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(IndexOutOfRangeException))]
        [InlineData(typeof(SystemException))]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Action_IsSelective(Type type)
        {
            _FakeAction.SetUpExceptions(1, type);
            Func<Task> @do = async () =>
                await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeAction, CancellationToken.None);
            var exceptionAssertions = await @do.Should().ThrowAsync<Exception>();
            exceptionAssertions.Where(ex => ex.GetType() == type);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTask.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource, typeof(ArgumentException));
            Func<Task> @do = async () => await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeTask, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTask.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Task_T__Calls_TillNoException(int times, int value)
        {
            _FakeTaskT.SetUpExceptions(times, value, typeof(ArgumentException));
            var res = await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeTaskT);

            res.Should().Be(value);
            await _FakeTaskT.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(4)]
        [InlineAutoData(10)]
        public async Task ForExceptionForEverWithTimeOut_Enqueue_Task_T_Can_BeCancelled(int times, int res)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTaskT.SetUpExceptionsWithCancellation(times, timesToCancel, res, tokenSource, typeof(ArgumentException));
            Func<Task> @do = async () => await _ForArgumentExceptionWithTimeOut.Enqueue(_FakeTaskT, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeTaskT.Received(timesToCancel + 1).Invoke();
        }

        #endregion

        #region Selective WithMaxRetry

        [Theory]
        [InlineData(0,1)]
        [InlineData(1,1)]
        [InlineData(5,10)]
        [InlineData(10, 20)]
        public async Task ForException_WithMax_Enqueue_Task_DoesNotThrow_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.For<NullReferenceException>().WithMaxRetry(maxRetry);
            _FakeTask.SetUpExceptions(times, typeof(NullReferenceException));
            Func<Task> @do = async () => await replay.Enqueue(_FakeTask);
            await @do.Should().NotThrowAsync();
            await _FakeTask.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, 1)]    
        [InlineData(10, 5)]
        [InlineData(1000, 3)]
        public async Task ForException_WithMax_Enqueue_Task_TillNoNullException_WhenLessThanMaxRetry(int times, int maxRetry)
        {
            var replay = RetryPolicy.For<NullReferenceException>().WithMaxRetry(maxRetry);
            _FakeTask.SetUpExceptions(times, typeof(NullReferenceException));
            Func<Task> @do = async () => await replay.Enqueue(_FakeTask);
            await @do.Should().ThrowAsync<NullReferenceException>();
            await _FakeTask.Received(maxRetry + 1).Invoke();
        }
        #endregion
    }
}
