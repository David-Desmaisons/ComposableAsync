using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using ComposableAsync.Resilient.Test.Helper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Resilient.Test
{
    public class RetryPolicyTests
    {
        private readonly IDispatcher _ForAllForEver;
        private readonly IDispatcher _ForNullReferenceExceptionForEver;

        private readonly Action _FakeAction;
        private readonly Func<Task> _FakeTask;
        private readonly Func<Task<int>> _FakeTaskT;

        public RetryPolicyTests()
        {
            _FakeAction = Substitute.For<Action>();
            _FakeTask = Substitute.For<Func<Task>>();
            _FakeTaskT = Substitute.For<Func<Task<int>>>();
            _ForAllForEver = RetryPolicy.ForAllException().ForEver();
            _ForNullReferenceExceptionForEver = RetryPolicy.For<NullReferenceException>().ForEver();
        }

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

        private class ChildNullReferenceException : NullReferenceException{ }

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
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            _FakeTask.SetUpExceptionsWithCancellation(times, timesToCancel, tokenSource, typeof(NullReferenceException));
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
            _FakeTaskT.SetUpExceptionsWithCancellation(times, timesToCancel, res, tokenSource, typeof(NullReferenceException));

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
    }
}
