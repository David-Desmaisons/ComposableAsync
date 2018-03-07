using System;

namespace EasyActor.TaskHelper
{
    internal struct TaskDescription
    {
        internal TaskType MethodType { get; set; }

        internal Type Type { get; set; }
    }
}
