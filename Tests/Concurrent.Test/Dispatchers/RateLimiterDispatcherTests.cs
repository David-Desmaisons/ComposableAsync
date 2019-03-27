using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Concurrent.Dispatchers;
using FluentAssertions;
using NSubstitute;
using RateLimiter;
using Xunit;
using Xunit.Abstractions;

namespace Concurrent.Test.Dispatchers
{
    public class RateLimiterDispatcherTests
    {
        private readonly RateLimiterDispatcher _RateLimiterDispatcher;
        private readonly ITestOutputHelper _TestOutput;
        private readonly IAwaitableConstraint _AwaitableConstraint;
        private readonly IDisposable _Disposable;
        private readonly Action _Action;
        private readonly Func<int> _Function;
        private readonly Func<Task> _FunctionTask;
        private readonly Func<Task<int>> _FunctionTaskInt;

        private const int Interval = 200;

        public RateLimiterDispatcherTests(ITestOutputHelper testOutput)
        {
            _TestOutput = testOutput;
            _AwaitableConstraint = Substitute.For<IAwaitableConstraint>();
            _Disposable = Substitute.For<IDisposable>();
            _AwaitableConstraint.WaitForReadiness(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(_Disposable));
            _Action = Substitute.For<Action>();
            _Function = Substitute.For<Func<int>>();
            _FunctionTask = Substitute.For<Func<Task>>();
            _FunctionTask().Returns(Task.CompletedTask);
            _FunctionTaskInt = Substitute.For<Func<Task<int>>>();

            _RateLimiterDispatcher = new RateLimiterDispatcher(_AwaitableConstraint);
        }

        [Fact]
        public void Dispatch_Calls_WaitForReadiness_And_Action()
        {
            _RateLimiterDispatcher.Dispatch(_Action);
            CheckSequence(_Action);
        }

        [Fact]
        public async Task Enqueue_Action_Calls_WaitForReadiness_And_Action()
        {
            await _RateLimiterDispatcher.Enqueue(_Action);
            CheckSequence(_Action);
        }

        [Fact]
        public async Task Enqueue_Func_Calls_WaitForReadiness_And_Action()
        {
            await _RateLimiterDispatcher.Enqueue(_Function);
            CheckSequence(() => _Function());
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Returns_Value_From_Function(int value)
        {
            _Function().Returns(value);
            var res = await _RateLimiterDispatcher.Enqueue(_Function);
            res.Should().Be(value);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Calls_WaitForReadiness_And_Function()
        {
            await _RateLimiterDispatcher.Enqueue(_FunctionTask);
            CheckSequence(() => _FunctionTask());
        }

        [Fact]
        public async Task Enqueue_Func_Task_Cancellation_Calls_WaitForReadiness_And_Function()
        {
            var cancellation = new CancellationToken(false);
            await _RateLimiterDispatcher.Enqueue(_FunctionTask, cancellation);
            CheckSequence(() => _FunctionTask(), cancellation);
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Task_Result_Calls_WaitForReadiness_And_Function(int value)
        {
            _FunctionTaskInt().Returns(Task.FromResult(value));
            await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt);
            CheckSequence(() => _FunctionTaskInt());
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Task_Result_Returns_Value_from_Task(int value)
        {
            _FunctionTaskInt().Returns(Task.FromResult(value));
            var res =await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt);
            res.Should().Be(value);
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Task_Result_Cancellation_Calls_WaitForReadiness_And_Function(int value)
        {
            var cancellation = new CancellationToken(false);
            _FunctionTaskInt().Returns(Task.FromResult(value));
            await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt, cancellation);
            CheckSequence(() => _FunctionTaskInt(), cancellation);
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Task_Result_Cancellation_Returns_Value_from_Task(int value)
        {
            var cancellation = new CancellationToken(false);
            _FunctionTaskInt().Returns(Task.FromResult(value));
            var res = await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt, cancellation);
            res.Should().Be(value);
        }

        private void CheckSequence(Action mainAction, CancellationToken? token = null)
        {
            var realToken = token ?? CancellationToken.None;
            Received.InOrder(() =>
            {
                _AwaitableConstraint.WaitForReadiness(realToken);
                mainAction();
                _Disposable.Dispose();
            });
        }

        [Fact]
        public async Task Dispatcher_Limit_Number_Of_Call()
        {
            var limiter = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(Interval));
            var rateLimiterDispatcher = new RateLimiterDispatcher(limiter);

            var count = 0;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(700));
            var stopWatch = Stopwatch.StartNew();
            _TestOutput.WriteLine($"Start: {DateTime.Now:O}");
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await rateLimiterDispatcher;

                if (cancellationTokenSource.IsCancellationRequested)
                    break;

                count++;
                _TestOutput.WriteLine($"Doing: {DateTime.Now:O}");
            }
            stopWatch.Stop();

            var maxNumberOfCall = (int) (Math.Truncate((decimal)stopWatch.ElapsedMilliseconds / Interval));
            var minNumberOfCall = 1;

            _TestOutput.WriteLine($"Ended: {DateTime.Now:O}");
            _TestOutput.WriteLine($"Count:{count}, Time spend: {stopWatch.Elapsed}");
            count.Should().BeInRange(minNumberOfCall, maxNumberOfCall);
        }
    }
}
