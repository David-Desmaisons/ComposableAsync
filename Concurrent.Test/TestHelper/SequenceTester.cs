using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Test.TestHelper
{
    public class SequenceTester
    {
        public int Count { get; private set; } = 0;
        public int MaxThreads { get; set; } = 500;
        private readonly IDispatcher _Dispatcher;

        public SequenceTester(IDispatcher dispatcher)
        {
            _Dispatcher = dispatcher;
        }

        public async Task Stress()
        {
            ParameterizedThreadStart safeAction = ctx =>
            {
                var completion = ctx as TaskCompletionSource<int>;
                _Dispatcher.Enqueue(() =>
                {
                    Thread.Sleep(2);
                    Count++;
                }).ContinueWith(_ => completion?.TrySetResult(0));
            };

            await PrivateStress(safeAction);
        }

        public async Task StressTask()
        {
            ParameterizedThreadStart safeAction = ctx =>
            {
                var completion = ctx as TaskCompletionSource<int>;
                _Dispatcher.Enqueue(async () =>
                {
                    await Task.Delay(2);
                    Count++;
                }).ContinueWith(_ => completion?.TrySetResult(0));
            };

            await PrivateStress(safeAction);
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
