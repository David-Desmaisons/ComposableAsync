using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public interface IConsumer<T>
    {
        Task Consume(T entry);
    }
}
