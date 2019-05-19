using System;
using System.Threading.Tasks;

namespace ComposableAsync.Factory
{
    /// <summary>
    ///  Factory to create proxy from POCO
    /// </summary>
    public interface IProxyFactory : IAsyncDisposable
    {
        /// <summary>
        ///  Build an proxy from a POCO
        ///  T should an interface through which the actor will be seen
        /// </summary>
        T Build<T>(T concrete) where T : class;

        /// <summary>
        ///  Build asynchronously an proxy from a POCO
        ///  using the factory context to call the function creating the POCO.
        ///  T should an interface through which the actor will be seen
        /// </summary>
        Task<T> BuildAsync<T>(Func<T> concrete) where T : class;
    }
}
