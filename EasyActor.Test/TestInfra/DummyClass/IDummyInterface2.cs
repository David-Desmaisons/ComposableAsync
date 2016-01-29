using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public interface IDummyInterface2 : IDummyInterface1
    {
        Task SlowDoAsync();

        Task<int> ComputeAsync(int value);

        Task<decimal> ComputeAndWaitAsync(decimal value);

        Task<Tuple<Thread, Thread>> DoAnRedoAsync();

        void Do();

        Task Throw();
    }
}
