using System;

namespace EasyActor.Pipeline
{
    public static class PipeLine
    {
        public static IPipeline<Tin, Tout> Create<Tin, Tout>(ITransformer<Tin, Tout> Init)
        {
            return new Pipeline<Tin, Tout>(Init);
        }

        public static IPipeline<Tin, Tout> Create<Tin, Tout>(Func<Tin, Tout> transform)
        {
            return Create(Transformer.Create(transform));
        }

        public static IPipeline<Tin, Tout> Create<Tin, Tout>(Func<Tin, Tout> transform, int MaxParallelism)
        {
            return Create(Transformer.Create(transform, MaxParallelism));
        }

        public static IClosedPipeline<T> Create<T>(IConsumer<T> transform)
        {
            return new ClosedPipeline<T>(transform);
        }
    
        public static IClosedPipeline<T> Create<T>(Action<T> transform)
        {
            return Create<T>(Consumer.Create(transform));
        }

        public static IClosedPipeline<T> Create<T>(Action<T> transform, int MaxParallelism)
        {
            return Create<T>(Consumer.Create(transform, MaxParallelism));
        }

        public static IPipeline<Tin, Tnext> Next<Tin, Tout, Tnext>(this IPipeline<Tin, Tout> @this, Func<Tout, Tnext> transform)
        {
            return @this.Next(Transformer.Create(transform));
        }

        public static IClosedPipeline<Tin> Next<Tin, Tout>(this IPipeline<Tin, Tout> @this, Action<Tout> transform)
        {
            return @this.Next(Consumer.Create(transform));
        }

        public static IPipeline<Tin, Tnext> Next<Tin, Tout, Tnext>(this IPipeline<Tin, Tout> @this, Func<Tout, Tnext> transform, int MaxParallelism)
        {
            return @this.Next(Transformer.Create(transform, MaxParallelism));
        }

        public static IClosedPipeline<Tin> Next<Tin, Tout>(this IPipeline<Tin, Tout> @this, Action<Tout> transform, int MaxParallelism)
        {
            return @this.Next(Consumer.Create(transform, MaxParallelism));
        }
    }
}
