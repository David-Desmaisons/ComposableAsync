using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyActor.TaskHelper;

namespace EasyActor.Disposable
{
    public class ActorDisposable
    {
        public static IAsyncDisposable StopableToAsyncDisposable(IActorLifeCycle lifeCycle)
        {
            return new AsyncDisposable(GetDisposable(lifeCycle));
        }

        public static IAsyncDisposable StopableToAsyncDisposable(IEnumerable<IActorLifeCycle> lifeCycle)
        {
            Func<Task> disposable = () => Task.WhenAll(lifeCycle.Select(act => GetDisposable(act)()).ToArray());
            return new AsyncDisposable(disposable);
        }

        private static Func<Task> GetDisposable(IActorLifeCycle lifeCycle) => 
                () => lifeCycle?.Stop() ?? TaskBuilder.Completed;
    }
}
