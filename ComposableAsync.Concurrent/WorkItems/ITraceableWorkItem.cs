using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.WorkItems
{
/// <summary>
    /// work item abstraction
    /// </summary>
    public interface ITraceableWorkItem: IWorkItem
    {
        /// <summary>
        /// Corresponding Task
        /// </summary>
        Task Task { get; }
    }

    /// <summary>
    /// WorkItem abstraction
    /// </summary>
    /// <typeparam name="T">result</typeparam>
    public interface ITraceableWorkItem<T> : IWorkItem
    {
        /// <summary>
        /// Corresponding Task
        /// </summary>
        Task<T> Task { get; }
    }
}
