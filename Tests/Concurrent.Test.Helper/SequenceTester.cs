using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Concurrent.Test.Helper
{
    public class SequenceTester
    {
        public int Count { get; private set; } = 0;
        public int MaxThreads { get; set; } = 500;
        private readonly IDispatcher _Dispatcher;
        private readonly ITestOutputHelper _TestOutputHelper;

        public SequenceTester(IDispatcher dispatcher, ITestOutputHelper testOutputHelper)
        {
            _Dispatcher = dispatcher;
            _TestOutputHelper = testOutputHelper;
        }

        public async Task Stress()
        {
            void SafeAction(object ctx)
            {
                var completion = ctx as TaskCompletionSource<int>;
                _Dispatcher.Enqueue(() =>
                    {
                        Thread.Sleep(2);
                        Count++;
                    })
                    .ContinueWith(_ => completion?.TrySetResult(0));
            }

            await PrivateStress(SafeAction);
        }

        public async Task StressTask()
        {
            void SafeAction(object ctx)
            {
                var completion = ctx as TaskCompletionSource<int>;
                _Dispatcher.Enqueue(async () =>
                    {
                        await Task.Delay(2);
                        Count++;
                    })
                    .ContinueWith(_ => completion?.TrySetResult(0));
            }


            var stoptWatch = Stopwatch.StartNew();
            await PrivateStress(SafeAction);

            stoptWatch.Stop();
            _TestOutputHelper?.WriteLine($"Time to run {MaxThreads} Enqueues in parrallel tasks: {stoptWatch.Elapsed}");
        }

        private async Task PrivateStress(ParameterizedThreadStart safeAction)
        {
            var range = Enumerable.Range(0, MaxThreads);
            var completions = range.Select(_ => new TaskCompletionSource<int>()).ToList();
            var thread = range.Select(_ => new Thread(safeAction)).ToList();
            var index = 0;
            thread.ForEach(t => t.Start(completions[index++]));
            thread.ForEach(t => t.Join());

            await Task.WhenAll(completions.Select(c => c.Task).ToArray());
        }
    }
}
