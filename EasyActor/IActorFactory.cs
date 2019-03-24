using System;
using System.Threading.Tasks;

namespace EasyActor
{
    /// <summary>
    ///  Factory to create actor from POCO
    /// </summary>
    public interface IActorFactory : IAsyncDisposable
    {
        /// <summary>
        ///  Build an actor from a POCO
        ///  T should an interface through which the actor will be seen
        /// </summary>
        T Build<T>(T concrete) where T : class;

        /// <summary>
        ///  Build asynchronously an actor from a POCO
        ///  using the actor thread to call the function creating the POCO.
        ///  T should an interface through which the actor will be seen
        /// </summary>
        Task<T> BuildAsync<T>(Func<T> concrete) where T : class;
    }
}
