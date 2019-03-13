namespace EasyActor.Options
{
    /// <summary>
    ///  Type of actor factory.
    /// </summary>
    public enum ActorFactorType
    {
        /// <summary>
        /// Standard factory, all created actors have a dedicated thread.
        /// </summary>
        Standard,

        /// <summary>
        /// Task Pool factory, actors will be executed sequentially on thread pool.
        /// </summary>
        TaskPool,

        /// <summary>
        /// Thread Pool factory, actors will be executed on a thread from the thread pool.
        /// </summary>
        ThreadPool,

        /// <summary>
        /// Shared factory, all created actors share the same thread.
        /// </summary>
        Shared,

        /// <summary>
        /// In Context factory: actors will use the current SynchronizationContext
        /// such as WPF or windows form thread.
        /// </summary>
        InCurrentContext,


        /// <summary>
        /// Actors will use the fiber provided to the actor factory
        /// </summary>
        FromFiber
    }
}
