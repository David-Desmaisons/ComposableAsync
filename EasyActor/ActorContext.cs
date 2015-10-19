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
    public class ActorContext
    {
        private readonly object _Proxy;
        public ActorContext(object proxy)
        {
            _Proxy = proxy;
        }

        public TaskFactory TaskFactory
        {
            get
            {
                SynchronizationContext synCon = ActorFactoryBase.GetContextFromProxy(_Proxy) ?? SynchronizationContext.Current;
                if (synCon == null)
                    return new TaskFactory();

                return new TaskFactory(new SynchronizationContextTaskScheduler(synCon));
            }
        }
    }
}
