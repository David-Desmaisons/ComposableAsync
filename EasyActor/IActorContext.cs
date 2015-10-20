using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface IActorContext
    {
        TaskFactory GetTaskFactory(object proxy);
    }
}
