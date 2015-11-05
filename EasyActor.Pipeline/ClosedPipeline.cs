using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class ClosedPipeline<T> : IClosedPipeline<T>
    {

        private Func<T, Task> _Process;

        public ClosedPipeline(IConsumer<T> Init)
        {
            _Process = Init.Consume;
        }


        public ClosedPipeline(Func<T, Task> Init)
        {
            _Process = Init;
        }

        public Task Consume(T entry)
        {
            return _Process(entry);
        }

        public IDisposable Connect(IObservable<T> source)
        {
            return source.Subscribe(t => _Process(t));
        }
    }
}
