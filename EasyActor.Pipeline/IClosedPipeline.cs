using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public interface IClosedPipeline<T> : IConsumer<T>
    {
        IDisposable Connect(IObservable<T> source);
    }
}
