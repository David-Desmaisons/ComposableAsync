using AutoFixture.Xunit2;
using Concurrent.Dispatchers;
using FluentAssertions;
using NSubstitute;
using RateLimiter;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync;
using Xunit;
using Xunit.Abstractions;

namespace Concurrent.Test.Dispatchers
{
    public class RateLimiterDispatcherTests
    {
        private readonly RateLimiterDispatcher _RateLimiterDispatcher;
        private readonly ITestOutputHelper _TestOutput;
        private readonly IRateLimiter _RateLimiter;
        private readonly Action _Action;
        private readonly Func<int> _Function;
        private readonly Func<Task> _FunctionTask;
        private readonly Func<Task<int>> _FunctionTaskInt;

        private const int Interval = 200;

        public RateLimiterDispatcherTests(ITestOutputHelper testOutput)
        {
            _TestOutput = testOutput;
            _RateLimiter = Substitute.For<IRateLimiter>();
            _Action = Substitute.For<Action>();
            _Function = Substitute.For<Func<int>>();
            _FunctionTask = Substitute.For<Func<Task>>();
            _FunctionTask().Returns(Task.CompletedTask);
            _FunctionTaskInt = Substitute.For<Func<Task<int>>>();

            _RateLimiterDispatcher = new RateLimiterDispatcher(_RateLimiter);
        }

        [Fact]
        public async Task Dispatch_Calls_Perform()
        {
            _RateLimiterDispatcher.Dispatch(_Action);

            await _RateLimiter.Received(1).Perform(Arg.Any<Action>());
            await _RateLimiter.Received().Perform(_Action);
        }

        [Fact]
        public async Task Enqueue_Action_Perform()
        {
            await _RateLimiterDispatcher.Enqueue(_Action);

            await _RateLimiter.Received(1).Perform(Arg.Any<Action>());
            await _RateLimiter.Received().Perform(_Action);
        }

        [Fact]
        public async Task Enqueue_Func_Calls_Perform()
        {
            await _RateLimiterDispatcher.Enqueue(_Function);

            await _RateLimiter.Received(1).Perform(Arg.Any<Func<int>>());
            await _RateLimiter.Received().Perform(_Function);
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Returns_Value_From_RateLimiter(int value)
        {
            _RateLimiter.Perform(_Function).Returns(Task.FromResult(value));

            var res = await _RateLimiterDispatcher.Enqueue(_Function);
            res.Should().Be(value);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Calls_Perform()
        {
            await _RateLimiterDispatcher.Enqueue(_FunctionTask);

            await _RateLimiter.Received(1).Perform(Arg.Any<Func<Task>>());
            await _RateLimiter.Received().Perform(_FunctionTask);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Cancellation_Calls_Perform()
        {
            var cancellation = new CancellationToken(false);
            await _RateLimiterDispatcher.Enqueue(_FunctionTask, cancellation);

            await _RateLimiter.Received(1).Perform(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _RateLimiter.Received().Perform(_FunctionTask, cancellation);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Result_Calls_Perform()
        {
            await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt);

            await _RateLimiter.Received(1).Perform(Arg.Any<Func<Task<int>>>());
            await _RateLimiter.Received().Perform(_FunctionTaskInt);
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Task_Result_Returns_Value_from_Task(int value)
        {
            _RateLimiter.Perform(_FunctionTaskInt).Returns(Task.FromResult(value));

            var res = await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt);
            res.Should().Be(value);
        }

        [Fact]
        public async Task Enqueue_Func_Task_Result_Cancellation_Calls_Perform()
        {
            var cancellation = new CancellationToken(false);
            await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt, cancellation);

            await _RateLimiter.Received(1).Perform(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
            await _RateLimiter.Received().Perform(_FunctionTaskInt, cancellation);
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_Task_Result_Cancellation_Returns_Value_from_Task(int value)
        {
            var cancellation = new CancellationToken(false);
            _RateLimiter.Perform(_FunctionTaskInt, cancellation).Returns(Task.FromResult(value));

            var res = await _RateLimiterDispatcher.Enqueue(_FunctionTaskInt, cancellation);
            res.Should().Be(value);
        }

        [Fact]
        public async Task Dispatcher_Limit_Number_Of_Call()
        {
            var limiter = TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(Interval));
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

            var maxNumberOfCall = (int)(Math.Truncate((decimal)stopWatch.ElapsedMilliseconds / Interval));
            var minNumberOfCall = 1;

            _TestOutput.WriteLine($"Ended: {DateTime.Now:O}");
            _TestOutput.WriteLine($"Count:{count}, Time spend: {stopWatch.Elapsed}");
            count.Should().BeInRange(minNumberOfCall, maxNumberOfCall);
        }
    }
}
