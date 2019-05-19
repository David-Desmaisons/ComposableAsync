using System;
using System.Threading.Tasks;

namespace ComposableAsync.Factory.Test.TestInfra.DummyClass
{
    public interface IDummyInterface1
    {
        Task DoAsync();

        Task DoAsync(IProgress<int> progress);
    }
}
