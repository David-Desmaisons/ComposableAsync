using ComposableAsync;

namespace EasyActor.Proxy
{
    internal class CancellableDispatcherProvider: ICancellableDispatcherProvider
    {
        internal CancellableDispatcherProvider(ICancellableDispatcher fiber)
        {
            Dispatcher = fiber;
        }

        public ICancellableDispatcher Dispatcher { get; }
    }
}
