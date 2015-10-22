using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EasyActor.Factories;
using EasyActor.TaskHelper;

namespace EasyActor
{
    public class ActorContext : IActorContext
    {
        public ActorContext()
        {
        }


        public TaskFactory GetTaskFactory(object proxy)
        {
            SynchronizationContext synCon = GetSynchronizationContext(proxy);
            if (synCon == null)
                return new TaskFactory();

            return new TaskFactory(new SynchronizationContextTaskScheduler(synCon));
        }


        public SynchronizationContext GetSynchronizationContext(object proxy)
        {
           return ActorFactoryBase.GetContextFromProxy(proxy) ?? SynchronizationContext.Current;
        }
    }
}
