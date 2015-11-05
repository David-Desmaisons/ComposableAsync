using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public static class Consumer
    {
        public static IConsumer<T> Create<T>(Action<T> consum)
        {
            return ActorBuilder.Factory.Build<IConsumer<T>>(new Consumer<T>(consum));
        }
    }
}
