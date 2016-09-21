using System;
using EasyActor.Flow.BackBone;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using System.Threading.Tasks;
using EasyActor.Flow.Processor;
using NSubstitute;
using EasyActor.Flow;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Xunit;

namespace EasyActor.FlowTest
{

    public class BackBoneTest: IDisposable
    {
        private IBackbone<bool, int> _BackBone;
        private IDictionary<Type, object> _Processors = new Dictionary<Type, object>();
        private IProcessor<bool, string, int> _StringProcessor;
        private IProcessor<bool, int, int> _IntProcessor;
        private IProgress<int> _Progess;
        private CancellationToken _CancellationToken;
        private IObservable<string> _SingleValue;
        private ObservableHelper _ObservableHelper;

        public BackBoneTest()
        {
            _CancellationToken = new CancellationToken();
            _StringProcessor = Substitute.For<IProcessor<bool, string, int>>();
            _IntProcessor = Substitute.For<IProcessor<bool, int, int>>();
            _Progess = Substitute.For<IProgress<int>>();
            _ObservableHelper = new ObservableHelper();
            _Processors = new Dictionary<Type, object>()
            {
                { typeof(string), _StringProcessor },
                { typeof(int), _IntProcessor }
            };
            _BackBone = new BackBone<bool, int>(_Processors);
            _SingleValue = Observable.Return("Value");
        }

        public void Dispose()
        {
            _BackBone.Dispose();
        }

        [Fact]
        public void Process_WithMessageOfUnknowType_ThrowException()
        {
            Func<Task> act = () => _BackBone.Process(new object(), _Progess, CancellationToken.None);
            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task Process_WithMessageTypeKnown_CallProcessor()
        {
            await _BackBone.Process("test", _Progess, _CancellationToken);
            await _StringProcessor.Received(1).Process("test", _BackBone, _Progess, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Process_CalledWithANullReferenceProgess_ProvidesANullProgress()
        {
            await _BackBone.Process("test", null, _CancellationToken);
            await _StringProcessor.Received(1).Process("test", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Process_OnCancelledBackBone_ThrowsException() 
        {
            _BackBone.Dispose();
            Func<Task> act = async() => await _BackBone.Process("test", _Progess, _CancellationToken);
            act.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public void Process_CalledWithCancelledToken_ThrowsException() 
        {
            var cancelledToken = new CancellationToken(true);
            Func<Task> act = async () => await _BackBone.Process("test", _Progess, cancelledToken);
            act.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public async Task Connect_SendObservedEvent_TotheProcessors()
        {
            _BackBone.Connect(_ObservableHelper.GetObservable());
            _ObservableHelper.Observe("Banana");

            await _StringProcessor.Received(1).Process("Banana", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Connect_AfterConnectResultDispose_DoNotSendObservedEventTotheProcessors()
        {
            using (_BackBone.Connect(_ObservableHelper.GetObservable())) { }
            _ObservableHelper.Observe("Banana");

            await _StringProcessor.DidNotReceive().Process("Banana", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Connect_AfterBackBoneDispose_DoNotSendObservedEventTotheProcessors()
        {
            _BackBone.Connect(_ObservableHelper.GetObservable());
            _BackBone.Dispose();
            _ObservableHelper.Observe("Banana");

            await _StringProcessor.DidNotReceive().Process("Banana", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Connect_AfterBackBoneDispose_DoNotSendObservedEventTotheProcessors_2()
        {
            _BackBone.Dispose();
            var disp =  _BackBone.Connect(_SingleValue);

            disp.Should().Be(Disposable.Empty);
            await _StringProcessor.DidNotReceive().Process("Value", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task GetObservable_SendMessageAsExpected_Result()
        {
            var observed = SetUpObservable<string>();
            await _BackBone.Process("string", null, CancellationToken.None);
            observed.Should().BeEquivalentTo("string");
        }

        [Fact]
        public async Task GetObservable_SendMessageAsExpected_Empty() 
        {
            var observed = SetUpObservable<string>();
            await _BackBone.Process(23, null, CancellationToken.None);
            observed.Should().BeEmpty();
        }

        private List<T> SetUpObservable<T>() 
        {
            var observed = new List<T>();
            var obs = _BackBone.GetObservableMessage<T>();
            obs.Subscribe(s => observed.Add(s));
            return observed;
        }
    }
}