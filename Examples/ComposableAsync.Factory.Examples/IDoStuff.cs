using System.Threading.Tasks;

namespace ComposableAsync.Factory.Examples
{
    public interface IDoStuff
    {
        Task DoStuff();

        Task<int> GetCount();
    }
}
