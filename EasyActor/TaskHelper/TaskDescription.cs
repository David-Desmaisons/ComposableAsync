using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.TaskHelper
{
    internal class TaskDescription
    {
        internal TaskType MethodType { get; set; }

        internal Type Type { get; set; }
    }
}
