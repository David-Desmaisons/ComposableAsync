using System;

namespace EasyActor.Pipeline
{
    public static class Transformer
    {
        public static ITransformer<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> transform)
        {
            return ActorBuilder.ActorFactory.Build<ITransformer<TIn, TOut>>(new Transformer<TIn, TOut>(transform));
        }

        public static ITransformer<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> transform, int maxParallelism)
        {
            return ActorBuilder.LoadBalancer.Build<ITransformer<TIn, TOut>>(()=>new Transformer<TIn, TOut>(transform), maxParallelism);
        }
    }
}
