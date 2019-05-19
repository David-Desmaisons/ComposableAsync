using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace ComposableAsync.Test.Helper
{
    public class PerfTimer : IDisposable
    {
        private readonly int _Count;
        private readonly Stopwatch _StopWatch;
        private readonly ITestOutputHelper _TestOutputHelper;

        public PerfTimer(int count, ITestOutputHelper testOutputHelper)
        {
            _Count = count;
            _TestOutputHelper = testOutputHelper;
            _StopWatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _StopWatch.Stop();
            var elapsed = _StopWatch.ElapsedMilliseconds;
            _TestOutputHelper.WriteLine("Elapsed: " + elapsed + " Actions: " + _Count);
            _TestOutputHelper.WriteLine("actions/ms: " + (_Count/elapsed));
        }
    }
}