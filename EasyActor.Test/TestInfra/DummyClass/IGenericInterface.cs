using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public interface IGenericInterface<T>
    {
        Task<T> GetResult(T inArgument);

        Task<TOther> GetResult<TOther>(TOther other);

        Task<string> GetResultString<TOther>(TOther other);
    }
}
