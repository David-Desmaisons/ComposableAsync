using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public interface IDummyInterface1
    {
        Task DoAsync();

        Task DoAsync(IProgress<int> Progress);
    }
}
