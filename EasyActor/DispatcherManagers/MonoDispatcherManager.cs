using System.Threading.Tasks;
using Concurrent;

namespace EasyActor.DispatcherManagers
{
    internal sealed class MonoDispatcherManager : IDispatcherManager
    {
        public bool DisposeDispatcher => false;
        private readonly ICancellableDispatcher _Dispatcher;

        public MonoDispatcherManager(ICancellableDispatcher dispatcher)
        {
           _Dispatcher = dispatcher;
        }

        public ICancellableDispatcher GetDispatcher() => _Dispatcher;

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
