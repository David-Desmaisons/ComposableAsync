using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Flow
{
    public interface IProcessorFinalizer<TRes, in TMessage, out TProgress>
    {
        Task<TRes> Process(TMessage message, IProgress<TProgress> progress, CancellationToken cancelationToken);
    }
}
