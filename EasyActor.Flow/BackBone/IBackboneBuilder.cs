using EasyActor.Flow.Processor;

namespace EasyActor.Flow.BackBone
{
    public interface IBackboneBuilder<TRes, TProgress>
    {
        void Register<TMessage>(IProcessor<TRes, TMessage, TProgress> processor);

        void Register<TMessage>(IProcessorFinalizer<TRes, TMessage, TProgress> processor);

        void Register<TMessage1, TMessage2>(ITransformProcessor<TMessage1, TMessage2, TProgress> processor);

        IBackbone<TRes, TProgress> GetBackBone();
    }
}
