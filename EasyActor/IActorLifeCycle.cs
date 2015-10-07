using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    public interface IActorLifeCycle
    {
        Task Abort();

        Task Stop();
    }
}
