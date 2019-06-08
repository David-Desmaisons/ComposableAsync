using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync
{
    public class CancellableDispatcherDelegatingHandler : DelegatingHandler
    {
        private readonly ICancellableDispatcher _Dispatcher;

        public CancellableDispatcherDelegatingHandler(ICancellableDispatcher dispatcher)
        {
            _Dispatcher = dispatcher;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _Dispatcher.Enqueue(() => base.SendAsync(request, cancellationToken), cancellationToken);
        }
    }
}
