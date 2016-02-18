using System;
using System.Threading.Tasks;
using EasyActor.Factories;
using EasyActor.Helper;

namespace EasyActor
{
    public class TaskPoolActorFactory : ActorFactoryBase, IActorFactory
    {
        public TaskPoolActorFactory()
        {
        }

        public override ActorFactorType Type
        {
            get { return ActorFactorType.TaskPool; }
        }

        private T Build<T>(T concrete, IStopableTaskQueue queue) where T : class
        {
            var asyncDisposable =  concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, queue, TypeHelper.IActorLifeCycleType,
                        new ActorLifeCycleInterceptor(queue, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, GetQueue());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetQueue();
            return queue.Enqueue( ()=> Build<T>(concrete(),queue) );
        }        
        
        private IStopableTaskQueue GetQueue()
        {
            return new TaskSchedulerQueue();
        }
    }
}
