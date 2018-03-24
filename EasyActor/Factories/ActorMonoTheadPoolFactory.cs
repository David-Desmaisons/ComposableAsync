using System;
using System.Threading.Tasks;
using Concurrent;

namespace EasyActor.Factories
{
    public abstract class ActorMonoTheadPoolFactory : ActorFactoryBase
    {
        protected abstract IStopableFiber GetMonoFiber();

        public T Build<T>(T concrete) where T : class
        {
            var cached = CheckInCache(concrete);
            return cached ?? Create<T>(concrete, GetMonoFiber());
        }

        public Task<T> BuildAsync<T>(Func<T> concrete) where T : class
        {
            var fiber = GetMonoFiber();
            return fiber.Enqueue(() => Create<T>(concrete(), fiber));
        }
    }
}
