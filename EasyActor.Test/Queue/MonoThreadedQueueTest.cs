using System;
using System.Threading;
using EasyActor.Queue;

namespace EasyActor.Test 
{

    public class MonoThreadQueueTest : MonoThreadedQueueBaseTest 
    {
        protected override IAbortableTaskQueue GetQueue(Action<Thread> onCreate = null)            
            => new MonoThreadedQueue(onCreate);
    }
}
