using EasyActor.TaskHelper;
using System;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class Consumer<T> :IConsumer<T>
    {
        private readonly Action<T> _act;
        public Consumer(Action<T> act)
        {
            _act = act;
        }

        public Task Consume(T entry)
        {
            _act(entry);
            return TaskBuilder.Completed;
        }
    }
}
