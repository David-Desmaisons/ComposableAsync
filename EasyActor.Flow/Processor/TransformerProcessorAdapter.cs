using EasyActor.Flow.BackBone;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.Processor
{
    internal class TransformerProcessorAdapter<TRes, TMessage1, TMessage2, TProgress> : IProcessor<TRes, TMessage1, TProgress>
    {
        private readonly ITransformProcessor<TMessage1, TMessage2, TProgress> _Transformer;

        internal TransformerProcessorAdapter(ITransformProcessor<TMessage1, TMessage2, TProgress> transformer)
        {
            _Transformer = transformer;
        }

        public async Task<TRes> Process(TMessage1 message, IBackbone<TRes, TProgress> backbone, IProgress<TProgress> progress, CancellationToken cancelationToken)
        {
            cancelationToken.ThrowIfCancellationRequested();
            var message2 = await  _Transformer.Transform(message, progress, cancelationToken);
            cancelationToken.ThrowIfCancellationRequested();
            return await backbone.Process(message2, progress, cancelationToken);
        }
    }
}
