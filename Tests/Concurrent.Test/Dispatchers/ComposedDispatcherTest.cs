using Concurrent.Dispatchers;
using NSubstitute;

namespace Concurrent.Test.Dispatchers
{
    public class ComposedDispatcherTest
    {
        private readonly IDispatcher _First;
        private readonly IDispatcher _Second;
        private readonly ComposedDispatcher _ComposedDispatcher;

        public ComposedDispatcherTest()
        {
            _First = Substitute.For<IDispatcher>();
            _Second = Substitute.For<IDispatcher>();
            _ComposedDispatcher = new ComposedDispatcher(_First, _Second);
        }
    }
}
