namespace ComposableAsync.Awaitable
{
    public struct DispatcherCancelableAwaitable
    {
        private readonly DispatcherCancelableAwaiter _DispatcherCancelableAwaiter;
        public DispatcherCancelableAwaitable(DispatcherCancelableAwaiter dispatcherCancelableAwaiter)
        {
            _DispatcherCancelableAwaiter = dispatcherCancelableAwaiter;
        }

        public DispatcherCancelableAwaiter GetAwaiter() => _DispatcherCancelableAwaiter;
    }
}
