using System;

namespace Concurrent.Tasks
{
    public struct TaskDescription
    {
        public TaskType MethodType { get; set; }

        public Type Type { get; set; }
    }
}
