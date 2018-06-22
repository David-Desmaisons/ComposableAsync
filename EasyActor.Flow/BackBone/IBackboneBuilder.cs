using EasyActor.Flow.Processor;

namespace EasyActor.Flow.BackBone
{
    public interface IBackboneBuilder<TRes, TProgress>
    {
        IBackboneBuilder<TRes, TProgress> Register<TMessage>(IProcessor<TRes, TMessage, TProgress> processor);

        IBackboneBuilder<TRes, TProgress> Register<TMessage>(IProcessorFinalizer<TRes, TMessage, TProgress> processor);

        IBackboneBuilder<TRes, TProgress> Register<TMessage1, TMessage2>(ITransformProcessor<TMessage1, TMessage2, TProgress> processor);

        IBackbone<TRes, TProgress> GetBackBone();
    }
}
