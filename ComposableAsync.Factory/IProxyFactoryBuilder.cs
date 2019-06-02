namespace ComposableAsync.Factory
{
    /// <summary>
    /// IProxyFactory and ILoadBalancerFactory factory
    /// </summary>
    public interface IProxyFactoryBuilder
    {
        /// <summary>
        /// Returns an proxy factory using the provided dispatcher
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(ICancellableDispatcher dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher1"></param>
        /// <param name="dispatcher2"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(ICancellableDispatcher dispatcher1, ICancellableDispatcher dispatcher2);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatchers"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(params ICancellableDispatcher[] dispatchers);

        /// <summary>
        /// Returns an proxy factory using the provided dispatcher
        /// Disposing the created factory will not dispose the provided dispatcher 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(ICancellableDispatcher dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher1"></param>
        /// <param name="dispatcher2"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(ICancellableDispatcher dispatcher1, ICancellableDispatcher dispatcher2);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatchers"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(params ICancellableDispatcher[] dispatchers);
    }
}
