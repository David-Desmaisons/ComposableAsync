using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EasyActor.Channel.Test
{
    public class ChannelTest
    {
        private readonly ITestOutputHelper _TestOutputHelper;
        public ChannelTest(ITestOutputHelper testOutputHelper)
        {
            _TestOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Fact()
        {
            var channel1 = new InChannel<int>();

            var t1 = Task.Run(() =>
            {
                Thread.Sleep(1000);
                channel1.OnNext(1);
                Thread.Sleep(1000);
                channel1.OnNext(2);
                channel1.OnCompleted();
            });

            var t2 = Task.Run(() =>
            {
                Thread.Sleep(200);
                channel1.OnNext(5);
                Thread.Sleep(1200);
                channel1.OnNext(50);
            });

            //await Task.WhenAll(new[] {t1, t2});

            await WatchChannel(channel1, CancellationToken.None);
        }

        private async Task WatchChannel(IOutChannel<int> channel, CancellationToken token)
        {
            var channel2 = channel.Transform(o => o.Select(v => (v * 2).ToString()));
            using (var enumerator = channel2.GetMessages())
            {
                while (await enumerator.MoveNext(token))
                {
                    var item = enumerator.Current;
                    _TestOutputHelper.WriteLine(item);
                }
            }
        }
    }
}
