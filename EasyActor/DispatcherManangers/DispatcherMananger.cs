using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.DispatcherManangers
{
    internal class DispatcherMananger : IDispatcherMananger
    {
        public ActorFactorType Type => ActorFactorType.FromFiber;
        public bool DisposeDispatcher => false;
        private readonly IDispatcher _Dispatcher;

        public DispatcherMananger(IDispatcher dispatcher)
        {
           _Dispatcher = dispatcher;
        }

        public IDispatcher GetDispatcher() => _Dispatcher;

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
