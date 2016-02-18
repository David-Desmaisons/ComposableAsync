namespace EasyActor
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
        /// Task Pool factory, actors will be executed sequencially on thread pool.
        /// </summary>
        TaskPool,

        /// <summary>
        /// Shared factory, all created actors share the same thread.
        /// </summary>
        Shared,

        /// <summary>
        /// In Context factory: actors will use the current SynchronizationContext
        /// such as WPF or windows form thread.
        /// </summary>
        InCurrentContext
    }
}
