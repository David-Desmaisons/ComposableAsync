using System;
namespace EasyActor
{
    /// <summary>
    ///  Factory to create actor from POCO
    /// </summary>
    public interface IActorFactory
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
    }
}
