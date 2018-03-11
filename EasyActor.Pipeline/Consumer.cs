using System;

namespace EasyActor.Pipeline
{
    public static class Consumer
    {
        public static IConsumer<T> Create<T>(Action<T> consum)
        {
            return ActorBuilder.ActorFactory.Build<IConsumer<T>>(new Consumer<T>(consum));
        }

        public static IConsumer<T> Create<T>(Action<T> consum, int maxParallelism)
        {
            return ActorBuilder.LoadBalancer.Build<IConsumer<T>>(() => new Consumer<T>(consum), maxParallelism);
        }
    }
}
