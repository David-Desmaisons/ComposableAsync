using EasyActor.TaskHelper;
using System;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class Consumer<T> : IConsumer<T>
    {
        private readonly Action<T> _Act;
        public Consumer(Action<T> act)
        {
            _Act = act;
        }

        public Task Consume(T entry)
        {
            _Act(entry);
            return TaskBuilder.Completed;
        }
    }
}
