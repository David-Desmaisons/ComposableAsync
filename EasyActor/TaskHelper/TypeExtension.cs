﻿using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.TaskHelper
{
    internal static class TypeExtension
    {

        internal static TaskDescription GetTaskType(this Type @this)
        {
            if (@this == typeof(Task))
                return new TaskDescription(){ MethodType = MethodType.Task };
            if (@this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Task<>))
                return new TaskDescription() {  Type = @this.GetGenericArguments()[0], MethodType = MethodType.GenericTask };
            return new TaskDescription() { MethodType = MethodType.None };
        }

    }
}
