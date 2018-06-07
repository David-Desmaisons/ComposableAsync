using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using EasyActor.Flow.BackBone;
using EasyActor.Flow.Processor;
using Xunit;

namespace EasyActor.Flow.Sample
{
    public class CreatingPipeline
    {

        private class Transformer : ITransformProcessor<double, string, int>
        {
            public Task<string> Transform(double message,  IProgress<int> progress, CancellationToken cancelationToken)
            {
                return Task.FromResult(message.ToString(CultureInfo.InvariantCulture));
            }
        }

        private class Parser : IProcessor<bool, string, int>
        {
            public async Task<bool> Process(string message, IBackbone<bool, int> backbone, IProgress<int> progress, CancellationToken cancelationToken)
            {
                int res;
                if (!int.TryParse(message, out res))
                    return false;

                return await backbone.Process(res, progress, cancelationToken);
            }
        }

        private class Counter : IProcessorFinalizer<bool, int, int>
        {
            private int _count = 0;
            public Task<bool> Process(int message, IProgress<int> progress, CancellationToken cancelationToken)
            {
                _count += message;
                return Task.FromResult(_count % 2 == 0);
            }
        }

        private readonly IBackbone<bool, int> _BackBone;

        public CreatingPipeline()
        {
            var builder = new BackBoneBuilder<bool, int>();
            builder.Register(new Transformer());
            builder.Register(new Parser());
            builder.Register(new Counter());
            _BackBone = builder.GetBackBone();
        }


        [Fact]
        public async Task Sample_Process_Int()
        {
            var res = await _BackBone.Process(20, null, CancellationToken.None);
            res.Should().BeTrue();
        }

        [Fact]
        public async Task Sample_Process_String_OK()
        {
            var res = await _BackBone.Process("23", null, CancellationToken.None);
            res.Should().BeFalse();
        }

        [Fact]
        public async Task Sample_Process_String_KO()
        {
            var res = await _BackBone.Process("fffff", null, CancellationToken.None);
            res.Should().BeFalse();
        }
    }
}
