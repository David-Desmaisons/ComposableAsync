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

    public interface IPingPonger
    {
        void Ping();
    }
}
