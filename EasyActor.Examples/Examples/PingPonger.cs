using System.Threading.Tasks;

using EasyActor.TaskHelper;

namespace EasyActor.Examples
{
    internal class PingPonger : IPingPonger
    {
        public int Count { get; set; }
        public string Name { get; private set; }

        internal IPingPonger Ponger { get; set; }

        public PingPonger(string iName)
        {
            Name = iName;
        }

        public Task Ping()
        {
             Count++;
            if (Ponger != null)
                Ponger.Ping();
            return TaskBuilder.Completed;
        }
    }
}
