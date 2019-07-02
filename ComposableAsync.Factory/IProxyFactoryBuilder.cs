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
        IProxyFactory GetManagedProxyFactory(IDispatcher dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher1"></param>
        /// <param name="dispatcher2"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(IDispatcher dispatcher1, IDispatcher dispatcher2);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatchers"></param>
        /// <returns></returns>
        IProxyFactory GetManagedProxyFactory(params IDispatcher[] dispatchers);

        /// <summary>
        /// Returns an proxy factory using the provided dispatcher
        /// Disposing the created factory will not dispose the provided dispatcher 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(IDispatcher dispatcher);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatcher1"></param>
        /// <param name="dispatcher2"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(IDispatcher dispatcher1, IDispatcher dispatcher2);

        /// <summary>
        /// Returns an proxy factory using the provided dispatchers sequentially
        /// Disposing the created factory will dispose the provided dispatcher
        /// </summary>
        /// <param name="dispatchers"></param>
        /// <returns></returns>
        IProxyFactory GetUnmanagedProxyFactory(params IDispatcher[] dispatchers);
    }
}
