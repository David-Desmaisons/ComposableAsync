using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Dispatchers;
using FluentAssertions;
using RateLimiter;
using Xunit;
using Xunit.Abstractions;

namespace Concurrent.Test.Dispatchers
{
    public class RateLimiterDispatcherTests
    {
        private readonly RateLimiterDispatcher _RateLimiterDispatcher;
        private readonly ITestOutputHelper _TestOutput;
        private const int Interval = 200;

        public RateLimiterDispatcherTests(ITestOutputHelper testOutput)
        {
            _TestOutput = testOutput;
            var limiter = new CountByIntervalAwaitableConstraint(1, TimeSpan.FromMilliseconds(Interval));
            _RateLimiterDispatcher = new RateLimiterDispatcher(limiter);
        }

        [Fact]
        public async Task Dispatcher_Limit_Number_Of_Call()
        {
            var count = 0;
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(700));
            var stopWatch = Stopwatch.StartNew();
            _TestOutput.WriteLine($"Start: {DateTime.Now:O}");
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await _RateLimiterDispatcher.SwitchToContext();

                if (cancellationTokenSource.IsCancellationRequested)
                    break;

                count++;
                _TestOutput.WriteLine($"Doing: {DateTime.Now:O}");
            }
            stopWatch.Stop();

            var numberOfCall = (int) (Math.Truncate((decimal)stopWatch.ElapsedMilliseconds / Interval));

            _TestOutput.WriteLine($"Ended: {DateTime.Now:O}");
            _TestOutput.WriteLine($"Count:{count}, Time spend: {stopWatch.Elapsed}");
            count.Should().Be(numberOfCall);
        }
    }
}
