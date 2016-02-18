using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///  Manage actor life cycle
    /// </summary>
    public interface IActorCompleteLifeCycle : IActorLifeCycle
    {
        /// <summary>
        ///  Terminate corresponding actor Thread.
        ///  All enqueued tasks will be cancelled and further access to
        ///  corresponding actor(s) will return cancelled tasks.
        ///  If corresponding actor(s) implement(s) <see cref="IAsyncDisposable"/>
        ///  interface the corresponding DisposeAsync will be called.
        /// </summary>
        /// <returns>
        /// A Task that completes when all the clean-up is done.
        /// </returns>
        Task Abort();
    }
}
