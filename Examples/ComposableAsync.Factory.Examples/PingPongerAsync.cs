using System.Threading.Tasks;

namespace ComposableAsync.Factory.Examples
{
    internal class PingPongerAsync : IPingPongerAsync
    {
        public int Count { get; set; }
        public string Name { get; }

        internal IPingPongerAsync PongerAsync { get; set; }

        public PingPongerAsync(string name)
        {
            Name = name;
        }

        public Task Ping()
        {
             Count++;
            PongerAsync?.Ping();
            return Task.CompletedTask;
        }
    }
}
