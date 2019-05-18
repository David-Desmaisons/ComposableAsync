﻿using System;

namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// Task description
    /// </summary>
    public struct TaskDescription
    {
        /// <summary>
        /// Task Type
        /// </summary>
        public TaskType MethodType { get; set; }

        /// <summary>
        /// Task generic type if any
        /// </summary>
        public Type Type { get; set; }
    }
}
