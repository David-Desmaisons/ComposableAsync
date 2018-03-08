using System;
using System.Threading;
using EasyActor.Fiber;

namespace EasyActor.Test.Fiber
{
    public class ThreadPoolFiberTest : MonoThreadedFiberBaseTest
    {
        protected override IMonoThreadFiber GetQueue(Action<Thread> onCreate = null) => new ThreadPoolFiber();
    }
}
