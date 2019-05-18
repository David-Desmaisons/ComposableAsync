using System.Threading.Tasks;

namespace EasyActor.Examples
{
    public interface IDoStuff
    {
        Task DoStuff();

        Task<int> GetCount();
    }
}
