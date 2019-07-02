using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Flow
{
    public interface IBackbone<TRes, out TProgress> : IDisposable
    {
        IDisposable Connect<TMessage>(IObservable<TMessage> source);

        Task<TRes> Process<TMessage>(TMessage message, IProgress<TProgress> progress, CancellationToken cancellationToken);

        IObservable<TMessage> GetObservableMessage<TMessage>();
    }
}
