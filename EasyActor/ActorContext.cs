using System.Threading.Tasks;
using EasyActor.Factories;

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
