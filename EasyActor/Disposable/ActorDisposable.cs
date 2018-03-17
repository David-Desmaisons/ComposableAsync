using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Concurrent.Tasks;

namespace EasyActor.Disposable
{
    public class ActorDisposable
    {
        public static IAsyncDisposable StopableToAsyncDisposable(IActorLifeCycle lifeCycle)
        {
            return new AsyncActionDisposable(GetDisposable(lifeCycle));
        }

        public static IAsyncDisposable StopableToAsyncDisposable(IEnumerable<IActorLifeCycle> lifeCycle)
        {
            Func<Task> disposable = () => Task.WhenAll(lifeCycle.Select(act => GetDisposable(act)()).ToArray());
            return new AsyncActionDisposable(disposable);
        }

        private static Func<Task> GetDisposable(IActorLifeCycle lifeCycle) => 
                () => lifeCycle?.Stop() ?? TaskBuilder.Completed;
    }
}
