using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Factory.Examples
{
    internal class PingPongerAsyncCancellable : IPingPongerAsyncCancellable
    {
        public int Count { get; set; }
        public string Name { get; }

        internal IPingPongerAsyncCancellable PongerAsync { get; set; }

        public PingPongerAsyncCancellable(string name)
        {
            Name = name;
        }

        public Task<bool> Ping(CancellationToken cancellationToken)
        {
            Count++;
            PongerAsync?.Ping(cancellationToken);
            return Task.FromResult(true);
        }
    }
}
