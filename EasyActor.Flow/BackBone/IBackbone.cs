using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.BackBone
{
    public interface IBackbone<TRes, TProgress> : IDisposable
    {
        IDisposable Connect<TMessage>(IObservable<TMessage> source);

        Task<TRes> Process<TMessage>(TMessage message, IProgress<TProgress> progress, CancellationToken cancellationToken);

        IObservable<TMessage> GetObservable<TMessage>();
    }
}
