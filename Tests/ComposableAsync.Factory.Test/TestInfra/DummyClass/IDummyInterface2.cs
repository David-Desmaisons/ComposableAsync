using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Factory.Test.TestInfra.DummyClass
{
    public interface IDummyInterface2 : IDummyInterface1
    {
        Task SlowDoAsync();

        Task<int> ComputeAsync(int value);

        Task<decimal> ComputeAndWaitAsync(decimal value);

        Task<Tuple<Thread, Thread>> DoAnRedoAsync();

        void Do();

        Task ThrowAsync(Exception exception = null);

        Task<int> ThrowAsyncWithResult(Exception exception = null);

        void Throw(Exception exception = null);
    }
}
