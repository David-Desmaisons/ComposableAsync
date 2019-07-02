using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Factory.Test.TestInfra.DummyClass
{
    public interface ICancellableInterface
    {
        Task<int> GetIntResult(int delay, CancellationToken cancellationToken);

        Task Do(int delay, CancellationToken cancellationToken);

        Task<TOther> GetResult<TOther>(int delay, TOther other, CancellationToken cancellationToken);
    }
}
