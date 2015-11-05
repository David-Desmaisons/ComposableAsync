using EasyActor.TaskHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class Consumer<T> :IConsumer<T>
    {
        private Action<T> _act;
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
