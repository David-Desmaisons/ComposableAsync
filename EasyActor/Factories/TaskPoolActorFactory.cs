﻿using System;
using System.Threading.Tasks;
using EasyActor.Helper;
using EasyActor.Proxy;
using EasyActor.Queue;

namespace EasyActor.Factories
{
    public class TaskPoolActorFactory : ActorFactoryBase, IActorFactory
    {
        public override ActorFactorType Type => ActorFactorType.TaskPool;

        private T Build<T>(T concrete, IStopableTaskQueue queue) where T : class
        {
            var asyncDisposable = concrete as IAsyncDisposable;
            return CreateIActorLifeCycle(concrete, queue, TypeHelper.ActorLifeCycleType,
                        new ActorLifeCycleInterceptor(queue, asyncDisposable));
        }

        public T Build<T>(T concrete) where T : class
        {
            return Build<T>(concrete, GetQueue());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var queue = GetQueue();
            return queue.Enqueue(() => Build<T>(concrete(), queue));
        }

        private IStopableTaskQueue GetQueue()
        {
            return new TaskSchedulerQueue();
        }
    }
}
