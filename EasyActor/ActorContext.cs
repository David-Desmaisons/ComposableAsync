using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    public class ActorContext
    {
        public ActorContext(object proxy)
        {
        }

        public TaskFactory TaskFactory { get; private set; }
    }
}
