namespace ComposableAsync.Factory
{
    /// <summary>
    /// <see cref="IDispatcher"/> extension
    /// </summary>
    public static class DispatcherExtension
    {
        /// <summary>
        /// Proxify a POCO using the <see cref="IDispatcher"/> as an asynchronous transformer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="concrete">Object to be proxified</param>
        /// <returns></returns>
        public static T Proxify<T>(this IDispatcher @this, T concrete) where T : class
        {
            var factory = new ProxyFactory(@this);
            return factory.Build<T>(concrete);
        }
    }
}
