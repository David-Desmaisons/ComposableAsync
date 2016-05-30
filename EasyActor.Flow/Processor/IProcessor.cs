using EasyActor.Flow.BackBone;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.Processor
{
    public interface IProcessor<TRes, in TMessage, TProgress>
    {
        Task<TRes> Process(TMessage message, IBackbone<TRes, TProgress> backbone, IProgress<TProgress> progress, CancellationToken cancellationToken);
    }
}
