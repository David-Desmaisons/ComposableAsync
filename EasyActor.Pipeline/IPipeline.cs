namespace EasyActor.Pipeline
{
    public interface IPipeline<Tin,Tout>
    {
        IPipeline<Tin, Tnext> Next<Tnext>(ITransformer<Tout, Tnext> next);

        IClosedPipeline<Tin> Next(IConsumer<Tout> next);

        IClosedPipeline<Tin> Next(params IConsumer<Tout>[] next);
    }
}
