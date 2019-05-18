namespace ComposableAsync.Concurrent.WorkItems
{
    /// <summary>
    /// work item
    /// </summary>
    public interface IWorkItem
    {
        /// <summary>
        /// Cancel action
        /// </summary>
        void Cancel();

        /// <summary>
        /// Execute work item
        /// </summary>
        void Do();
    }
}
