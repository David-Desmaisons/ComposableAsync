using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync
{
    public class DispatcherDelegatingHandler : DelegatingHandler
    {
        private readonly IDispatcher _Dispatcher;

        public DispatcherDelegatingHandler(IDispatcher dispatcher)
        {
            _Dispatcher = dispatcher;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _Dispatcher.Enqueue(() => base.SendAsync(request, cancellationToken));
        }
    }
}
