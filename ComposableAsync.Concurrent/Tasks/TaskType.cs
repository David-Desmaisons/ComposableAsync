namespace ComposableAsync.Concurrent
{
    /// <summary>
    /// Task description
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// None of the above
        /// </summary>
        None,

        /// <summary>
        /// Void type
        /// </summary>
        Void,

        /// <summary>
        /// None generic Task
        /// </summary>
        Task,

        /// <summary>
        /// Task<typeparam name="T"></typeparam>
        /// </summary>
        GenericTask,
    }
}
