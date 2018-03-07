using System;
using System.Threading.Tasks;

namespace EasyActor.TaskHelper
{
    internal static class TypeExtension
    {
        private static readonly Type _TaskType = typeof(Task);
        private static readonly Type _GenericTaskType = typeof(Task<>);
        private static readonly Type _VoidType = typeof(void);

        internal static TaskDescription GetTaskType(this Type @this)
        {
            if (@this == _VoidType)
                return new TaskDescription() { MethodType = TaskType.Void };

            if (@this == _TaskType)
                return new TaskDescription() { MethodType = TaskType.Task };

            if (@this.IsGenericType && @this.GetGenericTypeDefinition() == _GenericTaskType)
                return new TaskDescription() { Type = @this.GetGenericArguments()[0], MethodType = TaskType.GenericTask };

            return new TaskDescription() { MethodType = TaskType.None };
        }
    }
}
