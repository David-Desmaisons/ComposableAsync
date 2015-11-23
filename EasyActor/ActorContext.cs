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
            var synCon = GetTaskScheduler(proxy);
            return (synCon == null) ?  new TaskFactory() : new TaskFactory(synCon);
        }


        public TaskScheduler GetTaskScheduler(object proxy)
        {
           return ActorFactoryBase.GetContextFromProxy(proxy) ?? TaskScheduler.Current ?? TaskScheduler.Default;
        }
    }
}
