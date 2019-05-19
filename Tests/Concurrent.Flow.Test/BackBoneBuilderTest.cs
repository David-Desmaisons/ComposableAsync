using Concurrent.Flow.BackBone;
using Concurrent.Flow.Processor;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Concurrent.Flow.Test
{
    public class BackBoneBuilderTest
    {
        private readonly BackBoneBuilder<bool, int> _Builder;
        private IDictionary<Type, object> _Processors;
        private readonly IProcessor<bool, string, int> _Processor;
        private readonly IProgress<int> _Progress;
        private readonly ITransformProcessor<bool, string, int> _Transformer;
        private readonly IProcessorFinalizer<bool, int, int> _ProcessorFinalizer;
        private readonly IBackbone<bool, int> _BackBone;

        public BackBoneBuilderTest()
        {
            _Processors = new Dictionary<Type, object>();
            _Processor = Substitute.For<IProcessor<bool, string, int>>();
            _Transformer = Substitute.For<ITransformProcessor<bool, string, int>>();
            _ProcessorFinalizer = Substitute.For<IProcessorFinalizer<bool, int, int>>();
            _BackBone = Substitute.For<IBackbone<bool, int>>();
            _Progress = Substitute.For<IProgress<int>>();
            _Builder = new BackBoneBuilder<bool, int>(dic =>
            {
                _Processors = dic;
                return null;
            });
        }

        [Fact]
        public void Register_IProcessor_AddToDictionary()
        {
            _Builder.Register(_Processor);
            _Builder.GetBackBone();

            _Processors.Cast<KeyValuePair<Type, object>>().Should().BeEquivalentTo(new KeyValuePair<Type, object>[]
            {
                new KeyValuePair<Type, object>(typeof(string), _Processor)
            });
        }

        [Fact]
        public void Register_ITransformer_AddToDictionaryTransformerProcessorAdapter()
        {
            var boolTransformer = SetUpForTransformer();

            boolTransformer.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_ITransformer_AddToDictionaryCorrectTransformerProcessorAdapter()
        {
            var boolTransformer = SetUpForTransformer();
            await boolTransformer.Process(true, _BackBone, _Progress, CancellationToken.None);

            await _Transformer.Received(1).Transform(true, _Progress, CancellationToken.None);
        }

        private TransformerProcessorAdapter<bool, bool, string, int> SetUpForTransformer()
        {
            _Builder.Register(_Transformer);
            _Builder.GetBackBone();
            return _Processors[typeof(bool)] as TransformerProcessorAdapter<bool, bool, string, int>;
        }

        [Fact]
        public void Register_IProcessorFinalizer_AddToDictionaryFinalizerProcessorAdapter()
        {
            var boolTransformer = SetUpForFinalizer();

            boolTransformer.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_IProcessorFinalizer_AddToDictionaryCorrectFinalizerProcessorAdapter()
        {
            var boolTransformer = SetUpForFinalizer();
            await boolTransformer.Process(23, _BackBone, _Progress, CancellationToken.None);

            await _ProcessorFinalizer.Received(1).Process(23, _Progress, CancellationToken.None);
        }

        private ProcessorFinalizerAdapter<bool, int, int> SetUpForFinalizer()
        {
            _Builder.Register(_ProcessorFinalizer);
            _Builder.GetBackBone();
            return _Processors[typeof(int)] as ProcessorFinalizerAdapter<bool, int, int>;
        }
    }
}
