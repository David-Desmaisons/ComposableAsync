using Concurrent.Dispatchers;
using NSubstitute;

namespace Concurrent.Test.Dispatchers
{
    public class ComposedCancellableDispatcherTest 
    {
        private readonly ICancellableDispatcher _First;
        private readonly ICancellableDispatcher _Second;
        private readonly ComposedCancellableDispatcher _ComposedCancellableDispatcher;

        public ComposedCancellableDispatcherTest()
        {
            _First = Substitute.For<ICancellableDispatcher>();
            _Second = Substitute.For<ICancellableDispatcher>();
            _ComposedCancellableDispatcher = new ComposedCancellableDispatcher(_First, _Second);
        }
    }
}
