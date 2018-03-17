using System;
using System.Threading.Tasks;
using EasyActor.Disposable;
using EasyActor.Options;

namespace EasyActor
{
    /// <summary>
    ///  Factory to create actor from POCO
    /// </summary>
    public interface IActorFactory : IAsyncDisposable
    {
        /// <summary>
        ///  Returns the type of the factory.
        /// </summary>
        ActorFactorType Type { get; }

        /// <summary>
        ///  Build an actor from a POCO
        ///  T should an interface througth which the actor will be seen
        /// </summary>
        T Build<T>(T concrete) where T : class;

        /// <summary>
        ///  Build asynchroneously an actor from a POCO
        ///  using the actor thread to call the function creating the POCO.
        ///  T should an interface througth which the actor will be seen
        /// </summary>
        Task<T> BuildAsync<T>(Func<T> concrete) where T : class;
    }
}
