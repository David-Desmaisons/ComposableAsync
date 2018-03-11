using System;

namespace EasyActor.Pipeline
{
    public static class PipeLine
    {
        public static IPipeline<TIn, TOut> Create<TIn, TOut>(ITransformer<TIn, TOut> init)
        {
            return new Pipeline<TIn, TOut>(init);
        }

        public static IPipeline<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> transform)
        {
            return Create(Transformer.Create(transform));
        }

        public static IPipeline<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> transform, int maxParallelism)
        {
            return Create(Transformer.Create(transform, maxParallelism));
        }

        public static IClosedPipeline<T> Create<T>(IConsumer<T> transform)
        {
            return new ClosedPipeline<T>(transform);
        }
    
        public static IClosedPipeline<T> Create<T>(Action<T> transform)
        {
            return Create<T>(Consumer.Create(transform));
        }

        public static IClosedPipeline<T> Create<T>(Action<T> transform, int maxParallelism)
        {
            return Create<T>(Consumer.Create(transform, maxParallelism));
        }

        public static IPipeline<TIn, TNext> Next<TIn, TOut, TNext>(this IPipeline<TIn, TOut> @this, Func<TOut, TNext> transform)
        {
            return @this.Next(Transformer.Create(transform));
        }

        public static IClosedPipeline<TIn> Next<TIn, TOut>(this IPipeline<TIn, TOut> @this, Action<TOut> transform)
        {
            return @this.Next(Consumer.Create(transform));
        }

        public static IPipeline<TIn, TNext> Next<TIn, TOut, TNext>(this IPipeline<TIn, TOut> @this, Func<TOut, TNext> transform, int MaxParallelism)
        {
            return @this.Next(Transformer.Create(transform, MaxParallelism));
        }

        public static IClosedPipeline<TIn> Next<TIn, TOut>(this IPipeline<TIn, TOut> @this, Action<TOut> transform, int maxParallelism)
        {
            return @this.Next(Consumer.Create(transform, maxParallelism));
        }
    }
}
