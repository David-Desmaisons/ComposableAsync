using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class CancellableImplementation: ICancellableInterface
    {
        public Task<int> GetIntResult(int delay, CancellationToken cancellationToken)
        {
            Thread.Sleep(delay * 1000);
            return Task.FromResult(delay);
        }

        public Task Do(int delay, CancellationToken cancellationToken)
        {
            Thread.Sleep(delay * 1000);
            return Task.FromResult(delay);
        }

        public Task<TOther> GetResult<TOther>(int delay, TOther other, CancellationToken cancellationToken)
        {
            Thread.Sleep(delay * 1000);
            return Task.FromResult(other);
        }
    }
}
