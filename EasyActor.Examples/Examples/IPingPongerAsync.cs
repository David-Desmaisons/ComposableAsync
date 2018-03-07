using System.Threading.Tasks;

namespace EasyActor.Examples
{
    public interface IPingPongerAsync
    {
        Task Ping();
    }

    public interface IPingPonger
    {
        void Ping();
    }
}
