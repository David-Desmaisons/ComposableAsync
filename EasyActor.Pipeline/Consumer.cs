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
            return ActorBuilder.ActorFactory.Build<IConsumer<T>>(new Consumer<T>(consum));
        }

        public static IConsumer<T> Create<T>(Action<T> consum, int MaxParallelism)
        {
            return ActorBuilder.LoadBalancer.Build<IConsumer<T>>(() => new Consumer<T>(consum), MaxParallelism);
        }
    }
}
