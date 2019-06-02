namespace ComposableAsync
{
    /// <summary>
    /// <see cref="ICancellableDispatcherProvider"/> extension
    /// </summary>
    public static class CancellableDispatcherProviderExtension
    {
        /// <summary>
        /// Returns the underlying <see cref="IDispatcher"/>
        /// </summary>
        /// <param name="cancellableDispatcherProvider"></param>
        /// <returns></returns>
        public static IDispatcher GetAssociatedDispatcher(this ICancellableDispatcherProvider cancellableDispatcherProvider)
        {
            return cancellableDispatcherProvider?.Dispatcher ?? NullDispatcher.Instance;
        }
    }
}
