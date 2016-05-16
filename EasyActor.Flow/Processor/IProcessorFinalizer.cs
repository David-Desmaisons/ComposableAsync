using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.Processor
{
    public interface IProcessorFinalizer<TRes, TMessage, TProgress>
    {
        Task<TRes> Process(TMessage message, IProgress<TProgress> progress, CancellationToken cancelationToken);
    }
}
