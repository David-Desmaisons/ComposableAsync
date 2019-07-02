using System.Threading.Tasks;

namespace ComposableAsync.Factory.Examples
{
    internal class PingPongerBoolAsync : IPingPongerBoolAsync
    {
        public int Count { get; set; }
        public string Name { get; }

        internal IPingPongerBoolAsync PongerAsync { get; set; }

        public PingPongerBoolAsync(string name)
        {
            Name = name;
        }

        Task<bool> IPingPongerBoolAsync.Ping()
        {
            Count++;
            PongerAsync?.Ping();
            return Task.FromResult(true);
        }
    }
}
