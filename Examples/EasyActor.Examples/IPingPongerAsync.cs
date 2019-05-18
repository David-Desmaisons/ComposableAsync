using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Examples
{
    public interface IPingPongerAsync
    {
        Task Ping();
    }

    public interface IPingPongerBoolAsync
    {
        Task<bool> Ping();
    }

    public interface IPingPongerAsyncCancellable
    {
        Task<bool> Ping(CancellationToken cancellationToken);
    }

    public interface IPingPonger
    {
        void Ping();
    }
}
