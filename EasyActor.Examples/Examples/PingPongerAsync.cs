using System.Threading.Tasks;
using EasyActor.TaskHelper;

namespace EasyActor.Examples
{
    internal class PingPongerAsync : IPingPongerAsync
    {
        public int Count { get; set; }
        public string Name { get; }

        internal IPingPongerAsync PongerAsync { get; set; }

        public PingPongerAsync(string iName)
        {
            Name = iName;
        }

        public Task Ping()
        {
             Count++;
            PongerAsync?.Ping();
            return TaskBuilder.Completed;
        }
    }
}
