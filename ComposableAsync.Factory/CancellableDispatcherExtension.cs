namespace ComposableAsync.Factory
{
    /// <summary>
    /// 
    /// </summary>
    public static class CancellableDispatcherExtension
    {
        /// <summary>
        /// Proxify a POCO usando the <see cref="ICancellableDispatcher"/> as an ansynchroneous transformer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="concrete">Object to be proxified</param>
        /// <returns></returns>
        public static T Proxify<T>(ICancellableDispatcher @this, T concrete) where T : class
        {
            var factory = new ProxyFactory(@this);
            return factory.Build<T>(concrete);
        }
    }
}
