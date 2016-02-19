using System;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public interface IDummyInterface1
    {
        Task DoAsync();

        Task DoAsync(IProgress<int> Progress);
    }
}
