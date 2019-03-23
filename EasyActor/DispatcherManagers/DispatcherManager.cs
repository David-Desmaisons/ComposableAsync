using System.Threading.Tasks;
using Concurrent;
using EasyActor.Options;

namespace EasyActor.DispatcherManagers
{
    internal sealed class DispatcherManager : IDispatcherManager
    {
        public ActorFactorType Type => ActorFactorType.FromFiber;
        public bool DisposeDispatcher => false;
        private readonly ICancellableDispatcher _Dispatcher;

        public DispatcherManager(ICancellableDispatcher dispatcher)
        {
           _Dispatcher = dispatcher;
        }

        public ICancellableDispatcher GetDispatcher() => _Dispatcher;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
