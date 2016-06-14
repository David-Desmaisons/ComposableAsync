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
        private BackBone<bool, int> _BackBone;
        private IDictionary<Type, object> _Processors = new Dictionary<Type, object>();
        private IProcessor<bool, string, int> _Processor;
        private IProgress<int> _Progess;
        private CancellationToken _CancellationToken;
        private IObservable<string> _SingleValue;
        private ObservableHelper _ObservableHelper;

        public BackBoneTest()
        {
            _CancellationToken = new CancellationToken();
            _Processor = Substitute.For<IProcessor<bool, string, int>>();
            _Progess = Substitute.For<IProgress<int>>();
            _ObservableHelper = new ObservableHelper();
            _Processors = new Dictionary<Type, object>()
            {
                { typeof(string), _Processor }
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
            await _Processor.Received(1).Process("test", _BackBone, _Progess, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Process_CalledWithANullReferenceProgess_ProvidesANullProgress()
        {
            await _BackBone.Process("test", null, _CancellationToken);
            await _Processor.Received(1).Process("test", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
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

            await _Processor.Received(1).Process("Banana", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Connect_AfterConnectResultDispose_DoNotSendObservedEventTotheProcessors()
        {
            using (_BackBone.Connect(_ObservableHelper.GetObservable())) { }
            _ObservableHelper.Observe("Banana");

            await _Processor.DidNotReceive().Process("Banana", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Connect_AfterBackBoneDispose_DoNotSendObservedEventTotheProcessors()
        {
            _BackBone.Connect(_ObservableHelper.GetObservable());
            _BackBone.Dispose();
            _ObservableHelper.Observe("Banana");

            await _Processor.DidNotReceive().Process("Banana", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Connect_AfterBackBoneDispose_DoNotSendObservedEventTotheProcessors_2()
        {
            _BackBone.Dispose();
            var disp =  _BackBone.Connect(_SingleValue);

            disp.Should().Be(Disposable.Empty);
            await _Processor.DidNotReceive().Process("Value", _BackBone, Arg.Any<NullProgess<int>>(), Arg.Any<CancellationToken>());
        }
    }
}