using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///  Manage actor life cycle
    /// </summary>
    public interface IActorLifeCycle
    {
        /// <summary>
        ///  Terminate corresponding actor Thread.
        ///  All enqueued tasks will be cancelled and further access to
        ///  correspooding actor(s) will return cancelled tasks.
        ///  If corresponding actor(s) implement(s) <see cref="IAsyncDisposable"/>
        ///  interface the corresponding DisposeAsync will be called.
        /// </summary>
        /// <returns>
        /// A Task that completes when all the clean-up is done.
        /// </returns>
        Task Abort();

        /// <summary>
        ///  Terminate corresponding actor Thread.
        ///  All enqueued tasks will be processed and further access to
        ///  correspooding actor(s)  will return cancelled tasks.
        ///  If corresponding actor(s) implement(s) <see cref="IAsyncDisposable"/>
        ///  interface the corresponding DisposeAsync will be called.
        /// </summary>
        /// <returns>
        /// A Task that completes when all the clean-up is done.
        /// </returns>
        Task Stop();
    }
}
