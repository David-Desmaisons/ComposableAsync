using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using EasyActor.Queue;
using EasyActor.Factories;
using System.Threading;

namespace EasyActor
{
    public class TaskPoolActorFactory : ActorFactoryBase, IActorFactory
    {
        public TaskPoolActorFactory()
        {
        }

        public ActorFactorType Type
        {
            get { return ActorFactorType.TaskPool; }
        }

        private T Build<T>(T concrete, ITaskQueue queue) where T : class
        {
            return Create(concrete, queue);
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, GetQueue());
        }

        private ITaskQueue GetQueue()
        {
            var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            return new TaskSchedulerQueue(scheduler);
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetQueue();
            return queue.Enqueue( ()=> Build<T>(concrete(),queue) );
        }
    }
}
