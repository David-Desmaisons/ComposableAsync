using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public interface IConsumer<in T>
    {
        Task Consume(T entry);
    }
}
