using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Flow.Processor
{
    internal class ProcessorFinalizerAdapter<TRes, TMessage, TTProgress> : IProcessor<TRes, TMessage, TTProgress>
    {
        private readonly IProcessorFinalizer<TRes, TMessage, TTProgress> _Finalizer;

        internal ProcessorFinalizerAdapter(IProcessorFinalizer<TRes, TMessage, TTProgress> finalizer)
        {
            _Finalizer = finalizer;
        }

        public Task<TRes> Process(TMessage message, IBackbone<TRes, TTProgress> backbone, IProgress<TTProgress> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _Finalizer.Process(message, progress, cancellationToken);
        }
    }
}
