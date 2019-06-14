namespace ComposableAsync.Factory.Proxy
{
    internal class DispatcherProvider: IDispatcherProvider
    {
        internal DispatcherProvider(IDispatcher fiber)
        {
            Dispatcher = fiber;
        }

        public IDispatcher Dispatcher { get; }
    }
}
