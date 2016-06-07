using EasyActor.Flow.BackBone;
using EasyActor.Flow.Processor;
using NSubstitute;
 
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace EasyActor.FlowTest
{
    public class TransformerProcessorAdapterTest
    {
        private ITransformProcessor<double, string, int> _Transform;
        private IBackbone<bool, int> _BackBone;
        private TransformerProcessorAdapter<bool, double, string, int> _TransformerProcessorAdapter;
        private IProgress<int> _Progess;
        private CancellationTokenSource _CancellationTokenSource;

        public TransformerProcessorAdapterTest()
        {
            _Transform = Substitute.For<ITransformProcessor<double, string, int>>();
            _Transform.Transform(Arg.Any<double>(), Arg.Any<IProgress<int>>(), Arg.Any<CancellationToken>())
                      .Returns(x => Task.FromResult(string.Format("{0}", (double)x[0])));
            _BackBone = Substitute.For<IBackbone<bool, int>>();
            _Progess = Substitute.For<IProgress<int>>();
            _CancellationTokenSource = new CancellationTokenSource();
            _TransformerProcessorAdapter = new TransformerProcessorAdapter<bool, double, string, int>(_Transform);
        }

        [Fact]
        public async Task Process_CallITransformProcessorTransform()
        {
            await _TransformerProcessorAdapter.Process(0, _BackBone, _Progess, _CancellationTokenSource.Token);

            await _Transform.Received(1).Transform(0, _Progess, _CancellationTokenSource.Token);
        }

        [Fact]
        public async Task Process_CallBackBoneProcess()
        {
            await _TransformerProcessorAdapter.Process(0, _BackBone, _Progess, _CancellationTokenSource.Token);

            await _BackBone.Received(1).Process("0", _Progess, _CancellationTokenSource.Token);
        }

        [Fact]
        public async Task Process_WhenCancelled_DoNotCallITransformProcessorTransform()
        {
            _CancellationTokenSource.Cancel();

            Func<Task> act = () => _TransformerProcessorAdapter.Process(0, _BackBone, _Progess, _CancellationTokenSource.Token);
            act.ShouldThrow<TaskCanceledException>();

            await _Transform.DidNotReceive().Transform(0, _Progess, _CancellationTokenSource.Token);
        }
    }
}
