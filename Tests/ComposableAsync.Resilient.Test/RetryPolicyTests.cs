using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Resilient.Test
{
    public class RetryPolicyTests
    {
        private readonly IDispatcher _ForAllForEver;
        private readonly IDispatcher _ForNullReferenceExceptionForEver;

        private readonly Func<Task> _FakeAction;
        private readonly Func<Task<int>> _FakeFunc;

        public RetryPolicyTests()
        {
            _FakeAction = Substitute.For<Func<Task>>();
            _FakeFunc = Substitute.For<Func<Task<int>>>();
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
            SetUp(times);
            await _ForAllForEver.Enqueue(_FakeAction);
            await _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(NullReferenceException))]
        [InlineData(typeof(ArgumentException))]
        public async Task ForAllException_Enqueue_Task_CatchAllException(Type exceptionType)
        {
            SetUp(5, exceptionType);
            await _ForAllForEver.Enqueue(_FakeAction);
            await _FakeAction.Received(6).Invoke();
        }

        [Theory]
        [InlineAutoData(typeof(BadImageFormatException))]
        [InlineAutoData(typeof(NullReferenceException))]
        [InlineAutoData(typeof(ArgumentException))]
        public async Task ForAllException_Enqueue_Task_T_CatchAllException(Type exceptionType, int res)
        {
            SetUp(5, res, exceptionType);
            await _ForAllForEver.Enqueue(_FakeFunc);
            await _FakeFunc.Received(6).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForAllException_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            SetUp(times, timesToCancel, tokenSource);
            Func<Task> @do = async () => await _ForAllForEver.Enqueue(_FakeAction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeAction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForAllException_Enqueue_Task_T__Calls_TillNoException(int times, int value)
        {
            SetUp(times, value);
            var res = await _ForAllForEver.Enqueue(_FakeFunc);

            res.Should().Be(value);
            await _FakeFunc.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(4)]
        [InlineAutoData(10)]
        public async Task ForAllException_Enqueue_Task_T_Can_BeCancelled(int times, int res)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            SetUp(times, timesToCancel, res, tokenSource);
            Func<Task> @do = async () => await _ForAllForEver.Enqueue(_FakeFunc, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeFunc.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_TillNoNullException(int times)
        {
            SetUp(times, typeof(NullReferenceException));
            await _ForNullReferenceExceptionForEver.Enqueue(_FakeAction);
            await _FakeAction.Received(times + 1).Invoke();
        }

        private class ChildNullReferenceException : NullReferenceException{ }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_TillNoDerivedNullException(int times)
        {
            SetUp(times, typeof(ChildNullReferenceException));
            await _ForNullReferenceExceptionForEver.Enqueue(_FakeAction);
            await _FakeAction.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(10)]
        public async Task ForException_Enqueue_Task_Can_BeCancelled(int times)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            SetUp(times, timesToCancel, tokenSource, typeof(NullReferenceException));
            Func<Task> @do = async () => await _ForNullReferenceExceptionForEver.Enqueue(_FakeAction, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeAction.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(1)]
        [InlineAutoData(5)]
        [InlineAutoData(10)]
        public async Task ForException_Enqueue_Task_T_TillNoException(int times, int value)
        {
            SetUp(times, value, typeof(NullReferenceException));
            var res = await _ForNullReferenceExceptionForEver.Enqueue(_FakeFunc);

            res.Should().Be(value);
            await _FakeFunc.Received(times + 1).Invoke();
        }

        [Theory]
        [InlineAutoData(3)]
        [InlineAutoData(4)]
        [InlineAutoData(10)]
        public async Task ForException_Enqueue_Task_T_Can_BeCancelled(int times, int res)
        {
            var timesToCancel = 2;
            var tokenSource = new CancellationTokenSource();
            SetUp(times, timesToCancel, res, tokenSource, typeof(NullReferenceException));
            Func<Task> @do = async () => await _ForNullReferenceExceptionForEver.Enqueue(_FakeFunc, tokenSource.Token);
            await @do.Should().ThrowAsync<OperationCanceledException>();
            await _FakeFunc.Received(timesToCancel + 1).Invoke();
        }

        [Theory]
        [InlineData(typeof(BadImageFormatException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(Exception))]
        public async Task ForException_Enqueue_Only_Selected_Exception(Type exceptionType)
        {
            SetUp(1, exceptionType);
            Func<Task> @do = async () => await _ForNullReferenceExceptionForEver.Enqueue(_FakeAction);
            var expected = await @do.Should().ThrowAsync<Exception>();
            expected.Where(ex => ex.GetType() == exceptionType);
        }

        private void SetUp(int times, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            _FakeAction.Invoke().Returns(_ =>
            {
                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.CompletedTask;
            });
        }

        private void SetUp(int times, int cancelAfter, CancellationTokenSource tokenSource, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            _FakeAction.Invoke().Returns(_ =>
            {
                if (count == cancelAfter)
                    tokenSource.Cancel();

                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.CompletedTask;
            });
        }

        private void SetUp(int times, int result, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            _FakeFunc.Invoke().Returns(_ =>
            {
                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.FromResult(result);
            });
        }

        private void SetUp(int times, int cancelAfter, int result, CancellationTokenSource tokenSource, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            _FakeFunc.Invoke().Returns(_ =>
            {
                if (count == cancelAfter)
                    tokenSource.Cancel();

                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.FromResult(result);
            });
        }

    }
}
