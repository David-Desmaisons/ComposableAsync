using System;
using System.Threading;
using Concurrent.Fibers;

namespace Concurrent.Test.Fibers
{
    public class ThreadPoolFiberTest : MonoThreadedFiberBaseTest
    {
        protected override IMonoThreadFiber GetQueue(Action<Thread> onCreate = null) => new ThreadPoolFiber();
    }
}
