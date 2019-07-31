using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace ComposableAsync.Core.Test.Dispatchers
{
    public class ComposedDispatcherTest
    {
        private readonly IDispatcher _First;
        private readonly IDispatcher _Second;
        private readonly Action _Action;
        private readonly Func<Task> _FuncTask;
        private readonly Func<int> _FuncInt;
        private readonly Func<Task<int>> _FuncTaskInt;
        private readonly ComposedDispatcher _ComposedDispatcher;
        private Action _DispatchedAction;
        private Func<Task> _DispatchedFuncTask;
        private Func<Task<int>> _DispatchedFuncTaskInt;
        private readonly CancellationToken _CancellationToken = new CancellationToken();

        public ComposedDispatcherTest()
        {
            _First = Substitute.For<IDispatcher>();
            _First.Dispatch(Arg.Do<Action>(argument => _DispatchedAction = argument));
            _First.Enqueue(Arg.Do<Func<Task>>(argument => _DispatchedFuncTask = argument));
            _First.Enqueue(Arg.Do<Func<Task<int>>>(argument => _DispatchedFuncTaskInt = argument));
            _First.Enqueue(Arg.Do<Func<Task>>(argument => _DispatchedFuncTask = argument), Arg.Any<CancellationToken>());
            _First.Enqueue(Arg.Do<Func<Task<int>>>(argument => _DispatchedFuncTaskInt = argument), Arg.Any<CancellationToken>());

            _Second = Substitute.For<IDispatcher>();
            _Action = Substitute.For<Action>();
            _FuncInt = Substitute.For<Func<int>>();
            _FuncTask = Substitute.For<Func<Task>>();
            _FuncTaskInt = Substitute.For<Func<Task<int>>>();
            _ComposedDispatcher = new ComposedDispatcher(_First, _Second);
        }

        [Fact]
        public void Dispatch_Calls_Dispatch_First_On_First_Dispatcher()
        {
            _ComposedDispatcher.Dispatch(_Action);

            _First.Received(1).Dispatch(Arg.Any<Action>());
            _Second.DidNotReceive().Dispatch(Arg.Any<Action>());
        }

        [Fact]
        public void Dispatch_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            _ComposedDispatcher.Dispatch(_Action);
            _DispatchedAction();

            _Second.Received(1).Dispatch(Arg.Any<Action>());
            _Second.Received().Dispatch(_Action);
        }

        [Fact]
        public async Task Enqueue_Action_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedDispatcher.Enqueue(_Action);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task>>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Action>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>());
        }

        [Fact]
        public async Task Enqueue_Action_Calls_Enqueue_On_Second_Dispatcher_Then()
        {
            await _ComposedDispatcher.Enqueue(_Action);
            await _DispatchedFuncTask();

            await _Second.Received(1).Enqueue(Arg.Any<Action>());
            await _Second.Received().Enqueue(_Action);
        }

        [Fact]
        public async Task Enqueue_Action_With_Cancellation_Calls_Dispatch_First_On_First_Dispatcher()
        {
            var token = new CancellationToken();
            await _ComposedDispatcher.Enqueue(_Action, token);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _First.Received(1).Enqueue(Arg.Any<Func<Task>>(), token);

            await _Second.DidNotReceive().Enqueue(Arg.Any<Action>(), Arg.Any<CancellationToken>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Enqueue_Action_With_Cancellation_Calls_Enqueue_On_Second_Dispatcher_Then()
        {
            var token = new CancellationToken();
            await _ComposedDispatcher.Enqueue(_Action, token);
            await _DispatchedFuncTask();

            await _Second.Received(1).Enqueue(Arg.Any<Action>(), Arg.Any<CancellationToken>());
            await _Second.Received().Enqueue(_Action, token);
        }

        [Fact]
        public async Task Enqueue_Func_T_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedDispatcher.Enqueue(_FuncInt);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task<int>>>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Action>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task<int>>>());
        }

        [Fact]
        public async Task Enqueue_Func_T_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedDispatcher.Enqueue(_FuncInt);
            await _DispatchedFuncTaskInt();

            await _Second.Received(1).Enqueue(Arg.Any<Func<int>>());
            await _Second.Received().Enqueue(_FuncInt);
        }

        [Fact]
        public async Task Enqueue_Func_T_With_Cancellation_Calls_Dispatch_First_On_First_Dispatcher()
        {
            var token = new CancellationToken();
            await _ComposedDispatcher.Enqueue(_FuncInt, token);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
            await _First.Received(1).Enqueue(Arg.Any<Func<Task<int>>>(), token);

            await _Second.DidNotReceive().Enqueue(Arg.Any<Action>(), Arg.Any<CancellationToken>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Enqueue_Func_T_With_Cancellation_Calls_Enqueue_On_Second_Dispatcher_Then()
        {
            var token = new CancellationToken();
            await _ComposedDispatcher.Enqueue(_FuncInt, token);
            await _DispatchedFuncTaskInt();

            await _Second.Received(1).Enqueue(Arg.Any<Func<int>>(), Arg.Any<CancellationToken>());
            await _Second.Received().Enqueue(_FuncInt, token);
        }

        [Fact]
        public async Task Enqueue_FuncTask_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedDispatcher.Enqueue(_FuncTask);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task>>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Action>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>());
        }

        [Fact]
        public async Task Enqueue_FuncTask_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedDispatcher.Enqueue(_FuncTask);
            await _DispatchedFuncTask();

            await _Second.Received(1).Enqueue(Arg.Any<Func<Task>>());
            await _Second.Received().Enqueue(_FuncTask);
        }

        [Fact]
        public async Task Enqueue_FuncTask_T_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedDispatcher.Enqueue(_FuncTaskInt);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task<int>>>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Action>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>());
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task<int>>>());
        }

        [Fact]
        public async Task Enqueue_FuncTask_T_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedDispatcher.Enqueue(_FuncTaskInt);
            await _DispatchedFuncTaskInt();

            await _Second.Received(1).Enqueue(Arg.Any<Func<Task<int>>>());
            await _Second.Received().Enqueue(_FuncTaskInt);
        }

        [Fact]
        public async Task Enqueue_FuncTask_CancellationToken_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedDispatcher.Enqueue(_FuncTask, _CancellationToken);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _First.Received().Enqueue(Arg.Any<Func<Task>>(), _CancellationToken);
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Enqueue_FuncTask_CancellationToken_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedDispatcher.Enqueue(_FuncTask, _CancellationToken);
            await _DispatchedFuncTask();

            await _Second.Received(1).Enqueue(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await _Second.Received().Enqueue(_FuncTask, _CancellationToken);
        }

        [Fact]
        public async Task Enqueue_FuncTask_T_CancellationToken_Calls_Dispatch_First_On_First_Dispatcher()
        {
            await _ComposedDispatcher.Enqueue(_FuncTaskInt, _CancellationToken);

            await _First.Received(1).Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
            await _First.Received().Enqueue(Arg.Any<Func<Task<int>>>(), _CancellationToken);
            await _Second.DidNotReceive().Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Enqueue_FuncTask_T_CancellationToken_Calls_Dispatch_On_Second_Dispatcher_Then()
        {
            await _ComposedDispatcher.Enqueue(_FuncTaskInt, _CancellationToken);
            await _DispatchedFuncTaskInt();

            await _Second.Received(1).Enqueue(Arg.Any<Func<Task<int>>>(), Arg.Any<CancellationToken>());
            await _Second.Received().Enqueue(_FuncTaskInt, _CancellationToken);
        }
    }
}
