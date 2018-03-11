using EasyActor.Disposable;

namespace EasyActor.Pipeline
{
    public interface IPipeline<TIn,TOut> : IAsyncDisposable
    {
        IPipeline<TIn, TNext> Next<TNext>(ITransformer<TOut, TNext> next);

        IClosedPipeline<TIn> Next(IConsumer<TOut> next);

        IClosedPipeline<TIn> Next(params IConsumer<TOut>[] next);
    }
}
