using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Core.Test.Dispatchers
{
    public class ComposedCancellableDispatcherTest 
    {
        private readonly ICancellableDispatcher _First;
        private readonly ICancellableDispatcher _Second;
        private readonly Func<Task> _FuncTask;
        private readonly Func<Task<int>> _FuncTaskInt;
        private readonly ComposedCancellableDispatcher _ComposedCancellableDispatcher;
        private readonly CancellationToken _CancellationToken = new CancellationToken();

        private Func<Task> _DispatchedFuncTask;
        private Func<Task<int>> _DispatchedFuncTaskInt;

        public ComposedCancellableDispatcherTest()
        {
            _FuncTask = Substitute.For<Func<Task>>();
            _FuncTaskInt = Substitute.For<Func<Task<int>>>();

            _First = Substitute.For<ICancellableDispatcher>();
            _First.Enqueue(Arg.Do<Func<Task>>(argument => _DispatchedFuncTask = argument), Arg.Any<CancellationToken>());
            _First.Enqueue(Arg.Do<Func<Task<int>>>(argument => _DispatchedFuncTaskInt = argument), Arg.Any<CancellationToken>());

            _Second = Substitute.For<ICancellableDispatcher>();
            _ComposedCancellableDispatcher = new ComposedCancellableDispatcher(_First, _Second);
        }

        [Fact]
        public async Task Enqueue_FuncTask_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedCancellableDispatcher.Enqueue(_FuncTask, _CancellationToken);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _First.Received().Enqueue(Arg.Any<Func<Task>>(), _CancellationToken);
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Enqueue_FuncTask_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedCancellableDispatcher.Enqueue(_FuncTask, _CancellationToken);
            await _DispatchedFuncTask();

            await _Second.Received(1).Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _Second.Received().Enqueue(_FuncTask, _CancellationToken);
        }

        [Fact]
        public async Task Enqueue_FuncTask_T_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedCancellableDispatcher.Enqueue(_FuncTaskInt, _CancellationToken);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
            await _First.Received().Enqueue(Arg.Any<Func<Task<int>>>(), _CancellationToken);
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Enqueue_FuncTask_T_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedCancellableDispatcher.Enqueue(_FuncTaskInt, _CancellationToken);
            await _DispatchedFuncTaskInt();

            await _Second.Received(1).Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
            await _Second.Received().Enqueue(_FuncTaskInt, _CancellationToken);
        }
    }
}
