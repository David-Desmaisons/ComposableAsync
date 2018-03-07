using System;
using System.Threading;
using EasyActor.Queue;
using Retlang.Core;

namespace EasyActor.Test.Queue 
{
    public class MonoThreadRetlangQueueTest : MonoThreadedQueueBaseTest 
    {
        protected override IMonoThreadQueue GetQueue(Action<Thread> onCreate = null)
        {
            var queue = new BusyWaitQueue(new DefaultExecutor(), 100000, 30000);
            return new MonoThreadedRetlangQueue(queue, onCreate);
        }
    }
}
